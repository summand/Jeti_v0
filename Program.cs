/* Copyright (C) 2013 Interactive Brokers LLC. All rights reserved.  This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using IBApi;

namespace Jeti_v0
{
    public class program
    {
        public static int Main(string[] args)
        {
            //Connect to the API
            EWrapperImpl ApiWrapper = new EWrapperImpl();
            ApiWrapper.ClientSocket.eConnect("127.0.0.1", 7496, 0, false);

            // before proceding, monitor nextValidId
            while (ApiWrapper.NextOrderId <= 0) { }

            // Load securities
            var activecontracts = GetActiveContracts();

            // If no tickers found in database, load a default (hard coded) ticker
            if (activecontracts.Count == 0)
            {
                ActiveContract defaultcontract = new ActiveContract();
                DateTime date1 = new DateTime(1900, 12, 31, 0, 0, 0);
                defaultcontract.ActivityDate = date1;
                defaultcontract.IBFuturesLocalSymbol = "ZZN6";
                activecontracts.Add(defaultcontract);
            }

            // BUILD A CONTRACT FOR EACH TICKER
            // --http://stackoverflow.com/questions/10930705/creating-objects-dynamically-in-loop
            // --should build a dictionary of contract objects
            if (t.SecType == "FUT") 
            {
                Contract nextcontract = BuildFuturesContract(t);
            }
            else if (t.SecType == "STK")
            {
                Contract nextcontract = BuildUSStock(t);
            };

            // BEGIN DATA CAPTURE
            Console.WriteLine("BEGINNING DATA CAPTURE");
            int i = 0;
            Parallel.ForEach(activecontracts, (t) =>
            {
                i++;

                // Write list of Active Contract tickers
                Console.WriteLine(t.ActivityDate + ", " + t.IBFuturesLocalSymbol);

                //Request Real Time Bars for saving down to database
                ApiWrapper.ClientSocket.reqRealTimeBars(i, BuildFuturesContract(t), -1, "BID", false, GetFakeParameters(4));
                Thread.Sleep(5000);
            });

            //Signal Zoo - capture and calutulate signals on incoming data

            //Trade Selection - 

            ////Shut down           
            //Console.WriteLine("Disconnecting... Please press ENTER to close application.");
            //Console.ReadLine();
            //ApiWrappper.ClientSocket.eDisconnect();
            return 0;
        }

        public static Contract BuildFuturesContract(ActiveContract t)
        {
            Contract contract = new Contract();
            contract.SecType = "FUT";
            contract.Exchange = "NYMEX";
            contract.Currency = "USD";
            contract.LocalSymbol = "CLM6";
            return contract;
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