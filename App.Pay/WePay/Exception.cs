using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Pay.WePay
{
    public class WePayException : Exception
    {
        public WePayException(string msg) : base(msg)
        {

        }
    }
}
