using GroceryList.Lib.Models;
using System;

namespace GroceryList.Models.Data
{
    //public class SqlGroceryItem

    public class SqlGroceryItem : GroceryItem
    {
        //private DateTime? _createdOn, _inCartOn, _purchasedOn;
        //private int? _createdTz, inCartTz, _purchasedTz;
        //`created_on` datetime,
        public DateTime CreatedOn
        {
            get { return CreatedTime.UtcDateTime; }
            set { CreatedTime = new DateTimeOffset(value, CreatedTime.Offset); }
        }
        //`created_tz` int,
        public int CreatedTz
        {
            get { return (int)Math.Floor(CreatedTime.Offset.TotalSeconds); }
            set
            {
                var ts = new TimeSpan(0, 0, value);
                CreatedTime = new DateTimeOffset(CreatedTime.UtcDateTime, ts);
            }
        }
        //`in_cart_on` datetime,
        public DateTime? InCartOn
        {
            get { return InCartTime?.UtcDateTime; }
            set
            {
                if (value == null)
                {
                    InCartTime = null;
                    return;
                }
                InCartTime = new DateTimeOffset(value.Value, CreatedTime.Offset);
            }
        }
        //`in_cart_tz` int,
        public int? InCartTz
        {
            get; set;
            //get { return (int)Math.Floor(InCartTime.Offset.TotalSeconds); }
            //set
            //{
            //    var ts = new TimeSpan(0, 0, value);
            //    CreatedTime = new DateTimeOffset(CreatedTime.UtcDateTime, ts);
            //}
        }
        //`purchased_on` datetime,
        public DateTime? PurchasedOn
        {
            get { return PurchasedTime?.UtcDateTime; }
            set
            {
                if (value == null)
                {
                    PurchasedTime = null;
                    return;
                }
                PurchasedTime = new DateTimeOffset(value.Value, CreatedTime.Offset);
            }
        }
        //`purchased_tz` int,
        public int? PurchasedTz
        {
            get; set;
            //get { return (int)Math.Floor(PurchasedTime.Offset.TotalSeconds); }
            //set
            //{
            //    var ts = new TimeSpan(0, 0, value);
            //    CreatedTime = new DateTimeOffset(CreatedTime.UtcDateTime, ts);
            //}
        }
    }
}
