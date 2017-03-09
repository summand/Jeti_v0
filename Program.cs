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

        struct rtbUpdateStruct
        {
            internal string ticker;
            internal long time;
            internal double close;
        }

        static Queue q = new Queue();
        static Dictionary<string, int> reqIds = new Dictionary<string, int>();

        public static int Main(string[] args){

            //Connect to the API
            EWrapperImpl ApiWrapper = new EWrapperImpl();
            //public EWrapperImpl ApiWrapper;
            ApiWrapper.ClientSocket.eConnect("127.0.0.1", 7496, 0, false);

            // before proceding, monitor nextValidId
            while (ApiWrapper.NextOrderId <= 0) { }

            // Load securities
            var activecontracts = GetActiveContracts();

            // If no tickers found in database, load a default (hard coded) ticker
            if (activecontracts.Count == 0)
            {ActiveContract defaultcontract = new ActiveContract();
             DateTime date1 = new DateTime(1900, 12, 31, 0, 0, 0);
             defaultcontract.ActivityDate = date1;
             defaultcontract.IBFuturesLocalSymbol = "ZZN6";
             activecontracts.Add(defaultcontract);}

            // BUILD A CONTRACT FOR EACH TICKER
            var IBcontractlist = new List<Contract>();
            activecontracts.ForEach(t =>
            {if (t.ActivityDate == DateTime.Today)
                {Contract nextcontract = BuildNymexFuturesContract(t);
                    IBcontractlist.Add(nextcontract);}});

            // set up dataTable for capture of datafeed price data
            p.Columns.Add("ticker", typeof(string));
            p.Columns.Add("close", typeof(float));
            p.Columns.Add("time", typeof(DateTime));
            //p.Columns.Add("processed", typeof(DateTime));

            // set up dictionary of Tickers:ReqIds
            int i = 0;
            Parallel.ForEach(IBcontractlist, (t) =>
            {
                i++;
                reqIds.Add(t.LocalSymbol, i);
            });

            // open an api datafeed for each contract
            Parallel.ForEach(IBcontractlist, (t) =>
            {
                i++;
                ApiWrapper.ClientSocket.reqRealTimeBars(t.reqId, t, -1, "BID", false, 
                    GetFakeParameters(4));
                Thread.Sleep(5000);
            });
            
            // dequeue price updates and write to database
            while (1 > 0)
            {
                // if new data arrived...
                if (q.Count > 0)
                {
                    rtbUpdateStruct t = new rtbUpdateStruct();
                    t = (rtbUpdateStruct)q.Dequeue();

                    // write to console
                    Console.WriteLine(t.ticker +
                        ", close: " + t.close +
                        ", time: " + t.time);

                    // write to database
                    ApiWrapper.RealTimeBarCapturetoDB(t.close,t.time,t.ticker);

                    // append to datatable
                    p.Rows.Add(t.ticker, t.close, t.time);

                    // update technical indicators 
                    //Studies(t.ticker);
                    //Scores(t.ticker);
                    //Weights(t.ticker);
                    //Trades(t.ticker);
                    //Orders();
                    //Reports();
                }
            }
            
            ////Shut down           
            //Console.WriteLine("Disconnecting... Please press ENTER to close application.");
            //Console.ReadLine();
            //ApiWrappper.ClientSocket.eDisconnect();
            //return 0;
        }

        public static Contract BuildNymexFuturesContract(ActiveContract t)
        {
            Contract contract = new Contract();
            contract.SecType = "FUT";
            if (t.IBFuturesLocalSymbol.Substring(t.IBFuturesLocalSymbol.Length-2) == "ES")
            {contract.Exchange = "GLOBEX";}
            else {contract.Exchange = "NYMEX";}
            
            contract.Currency = "USD";
            contract.LocalSymbol = t.IBFuturesLocalSymbol;
            contract.reqId = Contract.GetActiveInstances();
            return contract;
        }

        public static void returnRTBfromAPI(int reqId, long time, double open, 
            double high, double low, double close, long volume, double WAP, int count)
        {   
            rtbUpdateStruct rtb = new rtbUpdateStruct();
            rtb.close = close;
            rtb.time = time;
            rtb.ticker = reqIds.FirstOrDefault(x => x.Value == reqId).Key;
            q.Enqueue(rtb);  
        }

        public static Contract BuildUSStock(ActiveContract t)
        {
            Contract contract = new Contract();
            contract.Symbol = "AMZN";
            contract.SecType = "STK";
            contract.Currency = "USD";
            contract.Exchange = "SMART";
            return contract;
        }

        static public List<ActiveContract> GetActiveContracts()
        {
            using (var jetiDB = new JETIEntities())
            {
                return (from ActiveContract in jetiDB.ActiveContracts
                        select ActiveContract).ToList();
            }
        }

        private static List<TagValue> GetFakeParameters(int numParams)
        {
            List<TagValue> fakeParams = new List<TagValue>();
            for (int i = 0; i < numParams; i++)
                fakeParams.Add(new TagValue("tag" + i, i.ToString()));
            return fakeParams;
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