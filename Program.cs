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
using System.Collections.Generic;
using IBApi;

namespace Jeti_v0{
    public class program{
        static bool rtbUpdateFlag;
        static Dictionary<string, dynamic> rtbVals;

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
                    //System.Diagnostics.Debug.WriteLine(nextcontract.LocalSymbol);
                    IBcontractlist.Add(nextcontract);}});

            // set up dictionary of Tickers:ReqIds
            Dictionary<string, int> reqIds = new Dictionary<string, int>();
       
            //---------------------------------------------------------------------------------
            // Capture Incoming Prices
            System.Diagnostics.Debug.WriteLine("---------------------------------------------");
            rtbUpdateFlag = false;
            Task dataCapture = new Task(() =>
                {while (1 != 0){
                    System.Diagnostics.Debug.WriteLine("**Test RTB Update:");
                    if (rtbUpdateFlag){
                            System.Diagnostics.Debug.WriteLine("rtbUpdate:");
                            System.Diagnostics.Debug.WriteLine(rtbVals["time"]);
                            System.Diagnostics.Debug.WriteLine("rtbUpdate:");
                            System.Diagnostics.Debug.WriteLine("rtbUpdate:");
                            System.Diagnostics.Debug.WriteLine("rtbUpdate:");
                            rtbUpdateFlag = false;
                        }
                    Thread.Sleep(700);
                    }
                });



            //---------------------------------------------------------------------------------
            // open data feeds and persist received data to database
            int i = 0;
            Parallel.ForEach(IBcontractlist, (t) =>
            {
                i++;
                reqIds.Add(t.LocalSymbol, i);
                System.Diagnostics.Debug.WriteLine("-----------------------------");
                System.Diagnostics.Debug.WriteLine(t.LocalSymbol);
                ApiWrapper.ClientSocket.reqRealTimeBars(t.reqId, t, -1, "BID", false, 
                    GetFakeParameters(4));                
                Thread.Sleep(5000);
            });



            //---------------------------------------------------------------------------------
            //Signal Zoo - capture and calutulate signals on incoming data
            //---------------------------------------------------------------------------------
            //Trade Selection - 
            //---------------------------------------------------------------------------------
            ////Shut down           
            //Console.WriteLine("Disconnecting... Please press ENTER to close application.");
            //Console.ReadLine();
            //ApiWrappper.ClientSocket.eDisconnect();
            return 0;
        }

        public static Contract BuildNymexFuturesContract(ActiveContract t)
        {
            Contract contract = new Contract();
            contract.SecType = "FUT";
            contract.Exchange = "NYMEX";
            contract.Currency = "USD";
            contract.LocalSymbol = t.IBFuturesLocalSymbol;
            contract.reqId = Contract.GetActiveInstances();

            return contract;
        }

        public static void returnRTBfromAPI(int reqId, long time, double open, 
            double high, double low, double close, long volume, double WAP, int count)
        {
            try { rtbVals.Clear(); }
            catch { }

            rtbVals.Add("reqId", reqId);
            rtbVals.Add("time", time);
            rtbVals.Add("open", open);
            rtbVals.Add("high", high);
            rtbVals.Add("low", low);
            rtbVals.Add("close", close);
            rtbVals.Add("volumne", volume);
            rtbVals.Add("wap", WAP);
            rtbVals.Add("count", count);
            rtbUpdateFlag = true;
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



        
    }
}