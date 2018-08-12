using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eval
{
    public static class Program
    {
        public static List<int> Users = new List<int>() { 92, 341, 477, 420, 62, 305, 398, 498, 17, 150, 194, 166, 139 };
        public static List<int> Moviecount = new List<int> { 11, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 13 };
        public static List<double> Fscore = new List<double>();

        static void Main(string[] args)
        {

            //GetList();
            List<double> CurrentEval = new List<double>();
            double Recall = 0;
            double Precision = 0;
            double fScore = 0;
            //double maxP = 0;
            //double minP = 100;
            //using (StreamWriter outputFile = new StreamWriter(Path.Combine(@"C:\Users\horcrux\Documents\Summer_2018\EvalFiles\", "top50_60train.txt"), true))
            //{
            //    outputFile.WriteLine("User, Recall, Precision, F1 Score");
            //    for (int i = 1; i <= 500; i++)
            //    {
            //        Cypher.SetFlagDefault(i);
            //        //Cypher.SetFlagAboveAverage(i);
            //        List<double> CurrentEval = Cypher.MoviesBasedOnUserProfile(i);//Cypher.MoviesBasedOnUserProfileAboveAvg(i);
            //        Recall = Math.Round(CurrentEval[0], 4);
            //        Precision = Math.Round(CurrentEval[1], 4);
            //        FScore = Math.Round(2 * ((Recall * Precision) / (Precision + Recall)), 4);
            //        outputFile.WriteLine(i + ", " + Recall + ", " + Precision + ", " + FScore+ ", TP: " + CurrentEval[2] + ", FP: " + CurrentEval[3] + ", FN: " + CurrentEval[4] + ", TN: " + CurrentEval[5]);
            //    }
            //}

            //for (int i = 1; i <= 500; i++)
            //{
            //    Cypher.SetFlagDefault(i);
            //    //Cypher.SetFlagAboveAverage(i);
            //    List<double> CurrentEval = Cypher.MoviesBasedOnUserProfile(i);//Cypher.MoviesBasedOnUserProfileAboveAvg(i);
            //    Recall = Math.Round(CurrentEval[0], 4);
            //    Precision = Math.Round(CurrentEval[1], 4);
            //    FScore = Math.Round(2 * ((Recall * Precision) / (Precision + Recall)), 4);
            //    Console.WriteLine(i + ", " + Recall + ", " + Precision + ", " + FScore);

            //    Console.WriteLine("Recall for user " + i + ": " + CurrentEval[0]);
            //    Console.WriteLine("Precision for user " + i + ": " + CurrentEval[1]);

            //}

            int user = 5;
            int movieid = 1;
            //using (StreamWriter outputFile = new StreamWriter(Path.Combine(@"C:\Users\horcrux\Documents\Summer_2018\EvalFiles\", "ContentOld_60_noLimit.txt"), true))
            //{
            //    outputFile.WriteLine("User, Recall, Precision, F1 Score");
            //    for (int i = 1; i <= 5; i++)
            //    {
            //        user = i;
            //        //Console.WriteLine("Evaluating collaborative recommendation for user profile " + user);
            //        Cypher.SetFlagDefault(user);
            //        //List<double> CurrentEval = Cypher.MoviesBasedOnUserProfile(user, movieid);
            //        List<double> CurrentEval = Cypher.ContentBasedEval(user, movieid);
            //        Recall = Math.Round(CurrentEval[0], 4);
            //        Precision = Math.Round(CurrentEval[1], 4);
            //        //Console.WriteLine("Recall: " + Recall);
            //        //Console.WriteLine("Precision: " + Precision);
            //        Double fScore = (2 * Recall * Precision) / (Precision + Recall);
            //        //Console.WriteLine("F-Score: " + fScore);
            //        outputFile.WriteLine("User " + i);
            //        outputFile.WriteLine("Confusion Matrix:");
            //        //TP FP
            //        //FN TN
            //        outputFile.WriteLine(CurrentEval[2] + " " + CurrentEval[3]);
            //        outputFile.WriteLine(CurrentEval[4] + " " + CurrentEval[5]);
            //        outputFile.WriteLine("Recall: " + Recall + "\nPrecision: " + Precision + "\nF1 Score: " + fScore);
            //        outputFile.WriteLine(" ");
            //    }
            //}


            //Console.WriteLine("Evaluating content-Based recommendation for user profile " + user + " for top 50 movies");
            //Cypher.SetFlagDefault(user);
            //CurrentEval = Cypher.ContentBasedEval(user, movieid);
            //Recall = Math.Round(CurrentEval[0], 4);
            //Precision = Math.Round(CurrentEval[1], 4);
            //fScore = (2 * Recall * Precision) / (Precision + Recall);
            //Console.WriteLine("Confusion Matrix:");
            ////TP FP
            ////FN TN
            //Console.WriteLine(CurrentEval[2] + " " + CurrentEval[3]);
            //Console.WriteLine(CurrentEval[4] + " " + CurrentEval[5]);
            //Console.WriteLine("Recall: " + Recall + " | Precision: " + Precision + "\nF1 Score: " + fScore);


            Console.WriteLine("Evaluating collaborative recommendation for user profile " + user + " for top 50 movies");
            Cypher.SetFlagDefault(user);
            CurrentEval = Cypher.MoviesBasedOnUserProfile(user, movieid);
            Recall = Math.Round(CurrentEval[0], 4);
            Precision = Math.Round(CurrentEval[1], 4);
            fScore = (2 * Recall * Precision) / (Precision + Recall);
            Console.WriteLine("Confusion Matrix:");
            //TP FP
            //FN TN
            Console.WriteLine(CurrentEval[2] + " " + CurrentEval[3]);
            Console.WriteLine(CurrentEval[4] + " " + CurrentEval[5]);
            Console.WriteLine("Recall: " + Recall + " | Precision: " + Precision + "\nF1 Score: " + fScore);


            //Console.WriteLine("Hello world");
            //Cypher.SetFlagDefault(56);
            //Cypher.SetFlagAboveAverage();

            //Console.WriteLine("Precision for user 56: " + Cypher.MoviesBasedOnUserProfile(56));
            ////Console.WriteLine("Precision: " + Cypher.MoviesBasedOnUserProfileAboveAvg()); 



            //Cypher.SetFlagDefault(1);
            //Console.WriteLine("Precision for user 1: " + Cypher.MoviesBasedOnUserProfile(1));

            //Cypher.SetFlagDefault(2);
            //Console.WriteLine("Precision for user 2: " + Cypher.MoviesBasedOnUserProfile(2));

            //Cypher.SetFlagDefault(3);
            //Console.WriteLine("Precision for user 3: " + Cypher.MoviesBasedOnUserProfile(3));

        }

        //public static void GetList()
        //{


        //    double Recall = 0;
        //    double Precision = 0;

        //    //int user = 388;
        //    foreach (int user in Users)
        //    {
        //        Cypher.SetFlagDefault(user);
        //        List<double> eval = Cypher.MoviesBasedOnUserProfile(user);
        //        Recall = eval[0];
        //        Precision = eval[1];
        //        double fScore = 2 * ((Recall * Precision) / (Precision + Recall));
        //        Fscore.Add(fScore);
        //    }
        //}
    }
}
