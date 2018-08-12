using Neo4jClient;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eval
{
    public class Cypher
    {

        public static IGraphClient GraphClient = GraphConfig.ConfigGraph();
        static List<int> ReservedMovies = new List<int>();
        static List<int> AboveAvgReservedMovies = new List<int>();
        

        public static List<double> MoviesBasedOnUserProfile(int userSK, int mSK)
        {
            List<double> EvalData = new List<double>();
            var Movies = GraphConfig.GraphClient.Cypher
                .Match("(p:UserDim { UserSK: {userSK}})<-[e: ratedUser]-(f:FactTable{flag:0})-[g: ratedMovie]->(h:MovieDim)")
                .With("SQRT(REDUCE(xDot = 0.0, a IN COLLECT(f.Rating - h.avgRate) | xDot + a^2)) AS xLength")
                .OptionalMatch("(p1:UserDim { UserSK: {userSK}})<-[x: ratedUser]-(f1:FactTable{flag:0})-[r1: ratedMovie]->(m:MovieDim)<-[r2: ratedMovie]-(f2:FactTable)-[y: ratedUser]->(p2:UserDim)")
                .With("xLength, collect(m.MovieSK) as mov, count(m) as movieCount, SUM((f1.Rating - m.avgRate) * (f2.Rating - m.avgRate)) AS xyDotProduct, " +
                "SQRT(REDUCE(yDot = 0.0, b IN COLLECT(f2.Rating - m.avgRate) | yDot + b ^ 2)) AS yLength, p1, p2")
                .WithParam("userSK", userSK)
                .With("p1, p2, xyDotProduct / (xLength* yLength) as similarity, mov")
                .OrderByDescending("similarity")
                .Limit(3)
                .OptionalMatch("(p2)<-[a: ratedUser]-(f3:FactTable)-[b: ratedMovie]->(m1:MovieDim)")
                .Where("NOT m1.MovieSK IN mov and m1.MovieSK <> {movieSK}")
                .WithParam("movieSK", mSK)
                .With("f3, mov, m1,  p2, round(100*avg(f3.Rating))/100 as avgRating")
                .Return((m1, avgRating) => new 
                {
                    movie = Return.As<MovieDim>("m1"),
                    averageRating = Return.As<double>("avgRating")
                })
                .OrderByDescending("f3.Rating")
                .Limit(50)
                .Results.ToList();
            
            //find TP, FP, FN and TN
            double TP =0, FP =0, FN =0, TN = 0;
            double Threshold = GetThreshold(userSK);
            List<FactTable> ReservedMovieDim = GetReserved(userSK);
            for (int k = 0; k < Movies.Count; k++)
            {
                int movieSK = Movies[k].movie.MovieSK;
                double movRat = Movies[k].averageRating;
                if (ReservedMovies.Contains(movieSK))
                {
                    FactTable movieItem = new FactTable();
                    foreach (FactTable resMov in ReservedMovieDim)
                    {
                        if (resMov.MovieSK == movieSK)
                        {
                            movieItem = resMov;
                        }
                    }
                    if (movRat >= Threshold)
                    {
                        if(movieItem.Rating >= Threshold)
                        {
                            TP += 1;
                        }
                        else
                        {
                            FP += 1;
                        }
                        
                    }
                    else //when similiar user avg rating is less then threshold
                    {
                        if (movieItem.Rating >= Threshold)
                        {
                            FN += 1;
                        }
                        else
                        {
                            TN += 1;
                        }
                    }
                    
                }
                else
                {
                   TN += 1;
                }
            }

            double recall = TP / (TP + FP);
            double precision = TP / (TP + FN);
            Console.WriteLine("TP: " + TP + ", FP: " + FP + ", FN: " + FN + ", TN: " + TN);

            EvalData.Add(recall);
            EvalData.Add(precision);
            EvalData.Add(TP);
            EvalData.Add(FP);
            EvalData.Add(FN);
            EvalData.Add(TN);

            //List<int> common = ReservedMovies.Intersect(Movies).ToList();
            //if (ReservedMovies.Count > 0)
            //{
            //    double recall = (common.Count * 100) / (ReservedMovies.Count);
            //    double precision = (common.Count * 100) / (Movies.Count);
            //    EvalData.Add((common.Count * 100) / (ReservedMovies.Count)); // Recall
            //    EvalData.Add((common.Count * 100) / (Movies.Count)); //Precision
            //}


            return EvalData;
        }


        public static List<double> ContentBasedEval(int userSK, int MovieSK)
        {
            List<double> EvalData = new List<double>();
            List<MovieDim> Movies = GraphConfig.GraphClient.Cypher
                .Match("(m: MovieDim{ MovieSK: {SK}})")
                .Match("(m) -[:belongsToGenre]->(g: GenreDim) < -[:belongsToGenre] - (simMov: MovieDim)")
                .Where("not(simMov)-[] - (: FactTable{ UserSK: {userSK}, flag: 0})")
                .With("m, simMov, COUNT(*) AS gCount")
                .OptionalMatch("(m)< -[:hasATag] - (a: TagDim) -[:hasATag]->(simMov)")
                .With("m, simMov, gCount, COUNT(a) AS tCount")
                .OptionalMatch("(m)< -[:ratedMovie] - (d: FactTable) -[:ratedMovie]->(simMov)")
                .With("m, simMov, gCount, tCount, COUNT(d) AS rCount")
                .With("simMov AS movies, (5* gCount)+(2* tCount)+(3* rCount) AS Weight")
                .WithParam("SK", MovieSK)
                .WithParam("userSK", userSK)
                .Return<MovieDim>("movies")
                .OrderByDescending("Weight")
                .Limit(50)
                .Results.ToList();


            //find TP, FP, FN and TN
            double TP = 0, FP = 0, FN = 0, TN = 0;
            double Threshold = GetThreshold(userSK);
            List<FactTable> ReservedMovieDim = GetReserved(userSK);
            foreach(MovieDim movie in Movies)
            {
                int movieSK = movie.MovieSK;
                double movRat = movie.avgRate;
                if (ReservedMovies.Contains(movieSK))
                {
                    FactTable movieItem = new FactTable();
                    foreach (FactTable resMov in ReservedMovieDim)
                    {
                        if (resMov.MovieSK == movieSK)
                        {
                            movieItem = resMov;
                        }
                    }
                    if (movRat >= Threshold)
                    {
                        if (movieItem.Rating >= Threshold)
                        {
                            TP += 1;
                        }
                        else
                        {
                            FP += 1;
                        }

                    }
                    else //when similiar user avg rating is less then threshold
                    {
                        if (movieItem.Rating >= Threshold)
                        {
                            FN += 1;
                        }
                        else
                        {
                            TN += 1;
                        }
                    }

                }
                else
                {
                    TN += 1;
                }
            }

            double recall = TP / (TP + FP);
            double precision = TP / (TP + FN);
            //Console.WriteLine("TP: " + TP + ", FP: " + FP + ", FN: " + FN + ", TN: " + TN);

            EvalData.Add(recall);
            EvalData.Add(precision);
            EvalData.Add(TP);
            EvalData.Add(FP);
            EvalData.Add(FN);
            EvalData.Add(TN);

            return EvalData;
        }

        public static double MoviesBasedOnUserProfileAboveAvg(int UserSK)
        {
            
            List<int> Movies = GraphConfig.GraphClient.Cypher
                .Match("(p:UserDim { UserSK: {UserSK}})<-[e: ratedUser]-(f:FactTable{flag:0})-[g: ratedMovie]->(h:MovieDim)")
                .With("SQRT(REDUCE(xDot = 0.0, a IN COLLECT(f.Rating - h.avgRate) | xDot + a^2)) AS xLength")
                .OptionalMatch("(p1:UserDim { UserSK: {UserSK}})<-[x: ratedUser]-(f1:FactTable{flag:0})-[r1: ratedMovie]->(m:MovieDim)<-[r2: ratedMovie]-(f2:FactTable)-[y: ratedUser]->(p2:UserDim)")
                .WithParam("UserSK", UserSK)
                .With("xLength, collect(m.MovieSK) as mov, count(m) as movieCount, SUM((f1.Rating - m.avgRate) * (f2.Rating - m.avgRate)) AS xyDotProduct, " +
                "SQRT(REDUCE(yDot = 0.0, b IN COLLECT(f2.Rating - m.avgRate) | yDot + b ^ 2)) AS yLength, p1, p2, " +
                "collect(f1.Rating) as rat1, collect(f2.Rating) as rat2")
                .With("p1, p2, xyDotProduct / (xLength* yLength) as similarity, mov")
                .OrderByDescending("similarity")
                .Limit(3)
                .OptionalMatch("(p2)<-[a: ratedUser]-(f3:FactTable)-[b: ratedMovie]->(m1:MovieDim)")               
                .With("f3, mov, m1")
                .Where("NOT m1.MovieSK IN mov and f3.Rating > m1.avgRate")
                .Return<int>("m1.MovieSK")
                .OrderByDescending("f3.Rating")
                //.Limit(100)
                .Results.ToList();


            List<int> common = ReservedMovies.Intersect(Movies).ToList();
            double precision = 0;
            if (ReservedMovies.Count > 0)
            {
                precision = (common.Count * 100) / (ReservedMovies.Count);
            }
            

            return precision;
        }

        public static void SetFlagDefault(int userSK)
        {
            GraphConfig.GraphClient.Cypher
                .Match("(p:UserDim { UserSK: {userSK}})<-[e: ratedUser]-(f:FactTable)")
                .WithParam("userSK", userSK)
                .Set("f.flag = 1")
                .ExecuteWithoutResults();

            //set 70 % of user as flag = 1
            List<int> MovieId = GraphConfig.GraphClient.Cypher
                .Match("(p:UserDim{ UserSK: {userSK}}) <-[e: ratedUser]-(f:FactTable)-[g: ratedMovie]->(h:MovieDim)")
                .WithParam("userSK", userSK)
                .Return<int>("h.MovieSK")
                .Results.ToList();
            int len = Convert.ToInt32(MovieId.Count() * (0.60));
            List<int> shortlist = MovieId.GetRange(0, len);
            ReservedMovies = MovieId.GetRange(len, MovieId.Count-len);

            AboveAvgReservedMovies = GraphConfig.GraphClient.Cypher
                .Match("(p:UserDim{ UserSK: {userSK}}) <-[e: ratedUser]-(f:FactTable)-[g: ratedMovie]->(h:MovieDim)")
                .WithParam("userSK", userSK)
                .Where("h.MovieSK in {ReservedMovies} and f.Rating>h.avgRate")
                .WithParam("ReservedMovies", ReservedMovies)
                .Return<int>("h.MovieSK")
                .Results.ToList();

            GraphConfig.GraphClient.Cypher
                .Match("(p:UserDim { UserSK: {userSK}})<-[e: ratedUser]-(f:FactTable)")
                .WithParam("userSK", userSK)
                .Where("f.MovieSK in {shortlist}")
                .WithParam("shortlist", shortlist)
                .Set("f.flag = 0")
                .ExecuteWithoutResults();

        }

       


        public static void SetFlagAboveAverage(int UserSK)
        {
            GraphConfig.GraphClient.Cypher
                .Match("(p:UserDim { UserSK: {UserSK}})<-[e: ratedUser]-(f:FactTable)")
                .WithParam("UserSK", UserSK)
                .Set("f.flag = 1")
                .ExecuteWithoutResults();

            //get above avg movies
            List<int> AboveAvgMovies = GraphConfig.GraphClient.Cypher
                .Match("(p:UserDim{ UserSK: {UserSK}}) <-[e: ratedUser]-(f:FactTable)-[g: ratedMovie]->(h:MovieDim)")
                .WithParam("UserSK", UserSK)
                .Where("f.Rating>h.avgRate")
                .Return<int>("h.MovieSK")
                .Results.ToList();


            int len = Convert.ToInt32(AboveAvgMovies.Count() * (0.5));
            ReservedMovies = AboveAvgMovies.GetRange(len, AboveAvgMovies.Count - len);

            //List<int> shortlist = GraphConfig.GraphClient.Cypher
            //    .Match("(p:UserDim { UserSK: {UserSK}})<-[e: ratedUser]-(f:FactTable)")
            //    .WithParam("UserSK", UserSK)
            //    .Where("NOT f.MovieSK in {movieList}")
            //    .WithParam("movieList", ReservedMovies)
            //    .Return<int>("f.MovieSK")
            //    .Results.ToList();

            GraphConfig.GraphClient.Cypher
                .Match("(p:UserDim { UserSK: {UserSK}})<-[e: ratedUser]-(f:FactTable)")
                .WithParam("UserSK", UserSK)
                .Where("NOT f.MovieSK in {movieList}")
                .WithParam("movieList", ReservedMovies)
                .Set("f.flag = 0")
                .ExecuteWithoutResults();

            //GraphConfig.GraphClient.Cypher
            //    .Match("(p:UserDim { UserSK: {UserSK}})<-[e: ratedUser]-(f:FactTable)")
            //    .WithParam("UserSK", UserSK)
            //    .WithParam("shortlist", shortlist)
            //    .Set("f.flag = 0")
            //    .ExecuteWithoutResults();
        }


        public static double GetThreshold(int userSK)
        {
            //find avg rating given by a user
            var Rating = GraphConfig.GraphClient.Cypher
                .Match("(p:UserDim{UserSK:{userSK}})<-[e: ratedUser]-(f:FactTable{flag:0})")
                .WithParam("userSK", userSK)
                .Return((f) => new
                {
                    rating = Return.As<String>("round(100*avg(f.Rating))/100")
                })
                .Results.ToList();

            return Convert.ToDouble(Rating[0].rating);
        }

        public static List<FactTable> GetReserved(int userSK)
        {
            return GraphConfig.GraphClient.Cypher
                .Match("(f:FactTable{UserSK:{userSK}})")
                .WithParam("userSK", userSK)
                .Where("f.MovieSK in {ReservedMovies}")
                .WithParam("ReservedMovies", ReservedMovies)
                .Return<FactTable>("f")
                .Results.ToList();
        }
    }



    public class MovieDim
    {
        public int MovieSK { get; set; }
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string ImdbId { get; set; }
        public string TmdbId { get; set; }
        public double avgRate { get; set; }
    }

    public class MovieCount
    {
        public string UserSK { get; set; }
        public int MovCount { get; set; }
    }

    public class RetrievedMovies
    {
        public MovieDim movie;
        public double averageRating; //this is average ratings of similar users
    }

    public class FactTable
    {
        public int MovieSK { get; set; }
        public int UserSK { get; set; }
        public int TimeSK { get; set; }
        public float Rating { get; set; }
        public int flag { get; set; }
    }

}
