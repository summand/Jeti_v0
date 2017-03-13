/* 
 * Jeti Trading System v0
 * Date 2017-03
 * Author: Daniel Siliski
 *  */
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using IBApi;
using System.Data;

namespace Jeti_v0{
    public class program{
        static DataTable p = new DataTable();
        static DataTable s = new DataTable();
        static Queue q = new Queue();
        static Dictionary<string, int> reqIds = new Dictionary<string, int>();
        struct rtbUpdateStruct{
            internal string ticker;
            internal long time;
            internal double close;}

        public static int Main(string[] args){

            // --------------------------------------------------------------------
            // Load ---------------------------------------------------------------
            //var activecontracts = GetActiveContracts();
            //var c = (from t in activecontracts
            //         where t.ActivityDate == DateTime.Today
            //         select t);
            var c = GetActiveContracts();

            Parallel.ForEach(c, (t) =>
                {
                    // Set up a repeating data import task
                    Task.Factory.StartNew(() => 
                    {
                        DataTable dt = new DataTable();                          
                        while (1 > 0)
                        {
                            Console.WriteLine("Task Tester! ... " + t.IBFuturesLocalSymbol);

                            dt = GetPriceData2(t);
                            foreach (DataRow r in dt.Rows)
                            {
                                foreach (var i in r.ItemArray)
                                {
                                    Console.WriteLine(i);

                                }
                            }

                            //var d = GetPriceData(t);
                            //foreach (var k in d)
                            //{
                            //    Console.WriteLine(k);
                            //}                           

                            Console.WriteLine(dt);
                            Thread.Sleep(10000);
                        }
                    });
                }
            );

            // --------------------------------------------------------------------
            // Transform ----------------------------------------------------------


            // --------------------------------------------------------------------
            // Save ---------------------------------------------------------------


            // --------------------------------------------------------------------
            // Quit ---------------------------------------------------------------
            //Console.WriteLine("disconnecting... please press enter to close application.");
            Console.ReadLine();
            return 0;
        }

        static public dynamic GetPriceData(ActiveContract t)
        {
        Console.WriteLine("--Getting Data For: " + t.IBFuturesLocalSymbol);
        using (var jetiDB = new JETIEntities())
        {
            return (from PriceCapture p in jetiDB.PriceCaptures
                    where p.Ticker == t.IBFuturesLocalSymbol
                      //&  p.IBTimestamp == DateTime.Today
                    select new {p.IBTimestamp, p.Ticker, p.Price }).ToList();
        }
        }

        static public DataTable GetPriceData2(ActiveContract t)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("time", typeof(DateTime));
            dt.Columns.Add("ticker", typeof(string));
            dt.Columns.Add("price", typeof(double));

            using (var jetiDB = new JETIEntities())
            {
                var q = from PriceCapture p in jetiDB.PriceCaptures
                        where p.Ticker == t.IBFuturesLocalSymbol
                        //&  p.IBTimestamp == DateTime.Today
                        select new { p.IBTimestamp, p.Ticker, p.Price };
                
                foreach (var k in q)
                {
                    dt.Rows.Add(k.IBTimestamp, k.Ticker, k.Price);
                    //Console.WriteLine("");
                }
            }
            return dt;
        }

        static public List<ActiveContract> GetActiveContracts()
        {
            using (var jetiDB = new JETIEntities())
            {
                return (from ActiveContract in jetiDB.ActiveContracts
                        where ActiveContract.ActivityDate == DateTime.Today
                        select ActiveContract).ToList();
            }
        }
        
        private static void Studies(string t)
        {
            // get data from p for t

            // calc 5period EWMA

            // calc 10period EWMA

            // calc 9period RSI

            // write values to a studies DataTable
        }

        private static void Scores(string t)
        {
            // calculate the score values of each contract
        }
        private static void Weights(string t)
        {
            // decide ideal portfolio weights (alpha forecast + t-cost estimate)
        }
        private static void Orders(string t)
        {
            // generate and execute API orders
        }
        private static void Reports(string t)
        {
            // persist data on trade price, pnl, positions, etc to database (and in future to reporting layer)
        }

        
    }
}