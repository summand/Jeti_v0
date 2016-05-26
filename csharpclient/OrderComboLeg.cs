/* Copyright (C) 2013 Interactive Brokers LLC. All rights reserved.  This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Jeti_v0
{
    /**
     * @class OrderComboLeg
     * @brief Allows to specify a price on an order's leg
     * @sa Order, ComboLeg
     */
    [ComVisible(true)]
    public class OrderComboLeg : Jeti_v0.TWSApi.IOrderComboLeg
    {
        
        double price;

        /**
         * @brief The order's leg's price
         */
        public double Price
        {
            get { return price; }
            set { price = value; }
        }

        public OrderComboLeg()
        {
            price = Double.MaxValue;
        }

        public OrderComboLeg(double p_price)
        {
            price = p_price;
        }

        public override bool Equals(Object other)
        {
            if (this == other)
            {
                return true;
            }
            else if (other == null)
            {
                return false;
            }

            OrderComboLeg theOther = (OrderComboLeg)other;

            if (price != theOther.Price)
            {
                return false;
            }

            return true;
        }

        double TWSApi.IOrderComboLeg.price { get { return this.Price; } set { this.Price = value; } }
    }
}