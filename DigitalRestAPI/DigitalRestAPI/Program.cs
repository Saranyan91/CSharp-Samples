using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace DigitalRestAPI
{
    class Program
    {
        #region Constants
        /// <summary>
        /// The EMS server to talk to.
        /// </summary>
        const string REST_HOST_URL = "developer.digitalpaytech.com";
        /// <summary>
        /// The account name created by the customer.
        /// </summary>
        const string TRANSACTION_REST_USER_NAME = "premiumparkingcoupon";
        const string COUPON_REST_USER_NAME = "premiumparkingcoupon";
        /// <summary>
        /// The secret assigned to the account by the EMS server when the account 
        /// was created. This should not be shared across applications or transmitted 
        /// in the clear.
        /// </summary>
        const string TRANSACTION_REST_SECRET = "W9RhGrjW0NODhBdm3abQakkfo0qjbY7Nh92qXHIf99c=";
        const string COUPON_REST_SECRET = "W9RhGrjW0NODhBdm3abQakkfo0qjbY7Nh92qXHIf99c=";
        #endregion

        static void Main(string[] args)
        {
            //TransactionPush(REST_HOST_URL, TRANSACTION_REST_USER_NAME, TRANSACTION_REST_SECRET);

            CouponPush(REST_HOST_URL, COUPON_REST_USER_NAME, COUPON_REST_SECRET);

            Console.ReadKey();
        }

        static void TransactionPush(string url, string username, string secret)
        {
            DigitalAPIOperations ops = new DigitalAPIOperations();

            ops.Host = url;
            ops.AccountName = username;
            ops.Secret = secret;

            //
            // Retrieve the session token.
            //
            Console.WriteLine("---Retrieving Session Token---");
            try
            {
                // The session token returned should be treated like a password and maintained in a secure fashion.
                ops.RetrieveSessionToken();
                Console.WriteLine("Session Token:" + ops.SessionToken);
                Console.WriteLine();
            }
            catch (WebException wex)
            {
                Console.WriteLine("Unable to retrieve session token:");
                // Note: The Status property is very helpful to anlayze problems programtically.
                Console.WriteLine(wex.Message + " (" + wex.Status.ToString() + ")");
                return;
            }

            //
            // List all the regions
            //
            Console.WriteLine("---Listing Regions---");
            try
            {
                XmlDocument regions = ops.RetrieveRegions();
                Console.WriteLine("Regions:");
                foreach (XmlNode region in regions.DocumentElement)
                {
                    Console.WriteLine(" \'" + region.InnerText + "\'");
                }
                Console.WriteLine();
            }
            catch (WebException wex)
            {
                Console.WriteLine("Unable to retrieve regions:");
                Console.WriteLine(wex.Message + " (" + wex.Status.ToString() + ")");
                Console.WriteLine();
            }

            //
            // Generate a unique permit number for the following create, update, and retrieve calls. The number 
            // needs to be unique to the account. It is recommended that for production, the number should 
            // increase as more transactions are created.
            //
            // For this exmaple we will use a random number. In your system, you should maintain a counter.
            //
            Random rand = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
            int permitNumber = rand.Next(99999);

            XmlDocument permitData = null;

            //
            // Create a permit
            //
            Console.WriteLine("---Creating New Permit---");
            try
            {
                permitData = GenerateNewPermitXml(permitNumber);
                // Note: The call may fail if the permit number generated matches a preexisting permit. When this 
                //       happens, the server will return a 412 "Failure - Object already exists" response code.
                ops.InsertNewPermit(permitData);

                Console.WriteLine("New Permit:");
                Console.WriteLine(FormatXml(permitData));
                Console.WriteLine();
            }
            catch (WebException wex)
            {
                Console.WriteLine("Unable to create permit:");
                Console.WriteLine(wex.Message + " (" + wex.Status.ToString() + ")");
                Console.WriteLine();
            }

            //
            // Retrieve the permit that was just created.
            //
            XmlDocument existingPermit = null;
            Console.WriteLine("---Retrieving Permit---");
            try
            {
                existingPermit = ops.RetrievePermit(permitNumber);
                if (existingPermit != null)
                {
                    Console.WriteLine("Permit just created:");
                    Console.WriteLine(FormatXml(existingPermit));
                    Console.WriteLine();
                }
            }
            catch (WebException wex)
            {
                Console.WriteLine("Unable to retrieve permit:");
                Console.WriteLine(wex.Message + " (" + wex.Status.ToString() + ")");
            }

            //
            // Extend permit
            //
            if (permitData != null)
            {
                Console.WriteLine("---Extending Permit---");

                // Simulate user extending the permit at a later date.
                System.Threading.Thread.Sleep(2000);

                try
                {
                    XmlDocument updatedPermit = GenerateExtendPermitXml(permitData);

                    ops.UpdatePermit(updatedPermit);
                    Console.WriteLine("Permit Extended:");
                    Console.WriteLine(FormatXml(updatedPermit));
                    Console.WriteLine();
                }
                catch (WebException wex)
                {
                    Console.WriteLine("Unable to extend permit:");
                    Console.WriteLine(wex.Message + " (" + wex.Status.ToString() + ")");
                    Console.WriteLine();
                }
            }

            //
            // Release Token
            //
            // It is important to release the token for security reasons and to release 
            // any resources being retained for that token.
            Console.WriteLine("---Release Token---");
            try
            {
                HttpStatusCode result = ops.ReleaseToken();
                Console.WriteLine("Token Released.");
                Console.WriteLine("Result:" + result.ToString());
                Console.WriteLine();
            }
            catch (WebException wex)
            {
                Console.WriteLine("Unable to release token:");
                Console.WriteLine(wex.Message + " (" + wex.Status.ToString() + ")");
                Console.WriteLine();
            }
        }

        static void CouponPush(string url, string username, string secret)
        {
            DigitalAPIOperations ops = new DigitalAPIOperations();

            ops.Host = url;
            ops.AccountName = username;
            ops.Secret = secret;

            //
            // Retrieve the session token.
            //
            Console.WriteLine("---Retrieving Session Token---");
            try
            {
                // The session token returned should be treated like a password and maintained in a secure fashion.
                ops.RetrieveSessionToken();
                Console.WriteLine("Session Token:" + ops.SessionToken);
                Console.WriteLine();
            }
            catch (WebException wex)
            {
                Console.WriteLine("Unable to retrieve session token:");
                // Note: The Status property is very helpful to anlayze problems programtically.
                Console.WriteLine(wex.Message + " (" + wex.Status.ToString() + ")");
                return;
            }

            //
            // List all the regions
            //
            Console.WriteLine("---Listing Regions---");
            try
            {
                XmlDocument regions = ops.RetrieveRegions();
                Console.WriteLine("Regions:");
                foreach (XmlNode region in regions.DocumentElement)
                {
                    Console.WriteLine(" \'" + region.InnerText + "\'");
                }
                Console.WriteLine();
            }
            catch (WebException wex)
            {
                Console.WriteLine("Unable to retrieve regions:");
                Console.WriteLine(wex.Message + " (" + wex.Status.ToString() + ")");
                Console.WriteLine();
            }

            Random rand = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
            string couponCode = "A" + rand.Next(99999).ToString().PadLeft(5, '0');
            HttpStatusCode result = HttpStatusCode.OK;
            XmlDocument requestData = null;
            XmlDocument responseData = null;

            //
            // Create Coupon
            //
            Console.WriteLine("---Adding a new coupon---");
            requestData = GetNewCouponXml(couponCode);
            result = ops.InsertCoupon(couponCode, requestData);
            Console.WriteLine("Response:" + result);
            Console.WriteLine();

            //
            // Update Coupon
            //
            Console.WriteLine("---Updating the coupon---");
            requestData = GetUpdateCouponXml(couponCode);
            result = ops.UpdateCoupon(couponCode, requestData);
            Console.WriteLine("Response:" + result);
            Console.WriteLine();

            //
            // Retrieve Coupon
            //
            Console.WriteLine("---Retreiving the updated coupon---");
            result = ops.RetrieveCoupon(couponCode, out responseData);
            Console.WriteLine("Response:" + result);
            if (result == HttpStatusCode.OK)
            {
                Console.WriteLine(FormatXml(responseData));
            }
            Console.WriteLine();

            //
            // Delete Permit
            //
            Console.WriteLine("---Deleting the coupon---");
            result = ops.DeleteCoupon(couponCode, out responseData);
            Console.WriteLine("Response:" + result);
            if (result == HttpStatusCode.OK)
            {
                Console.WriteLine(FormatXml(responseData));
            }
            Console.WriteLine();
            Console.WriteLine("---Deleting the coupon---");
            result = ops.DeleteCoupon("SARAN123", out responseData);
            Console.WriteLine("Response:" + result);
            if (result == HttpStatusCode.OK)
            {
                Console.WriteLine(FormatXml(responseData));
            }
            Console.WriteLine();
            Console.WriteLine("---Deleting the coupon---");
            result = ops.DeleteCoupon("SARAN691", out responseData);
            Console.WriteLine("Response:" + result);
            if (result == HttpStatusCode.OK)
            {
                Console.WriteLine(FormatXml(responseData));
            }
            Console.WriteLine();

            string[] couponCodes = new string[]
            {
                "A" + rand.Next(99999).ToString().PadLeft(5,'0'),
                "A" + rand.Next(99999).ToString().PadLeft(5,'0'),
                "A" + rand.Next(99999).ToString().PadLeft(5,'0'),
                "A" + rand.Next(99999).ToString().PadLeft(5,'0')
            };

            //
            // Create Multiple Coupons
            //
            Console.WriteLine("---Adding multiple coupons---");
            requestData = GetNewCouponsXml(couponCodes);
            result = ops.InsertCoupons(requestData, out responseData);
            Console.WriteLine("Response:" + result);
            if (result == HttpStatusCode.OK)
            {
                Console.WriteLine(FormatXml(responseData));
            }
            Console.WriteLine();

            //
            // Retrieve Multiple Coupons
            //
            Console.WriteLine("---Retreiving the created coupons---");
            string coupons = GetCouponList(couponCodes);
            result = ops.RetrieveCoupons(coupons, out responseData);
            Console.WriteLine("Response:" + result);
            if (result == HttpStatusCode.OK)
            {
                Console.WriteLine(FormatXml(responseData));
            }
            Console.WriteLine();

            //
            // Delete Multiple Coupons
            //
            Console.WriteLine("---Deleting the created coupon---");
            requestData = GetDeleteCouponsXml(couponCodes);
            result = ops.DeleteCoupons(requestData, out responseData);
            Console.WriteLine("Response:" + result);
            if (result == HttpStatusCode.OK)
            {
                Console.WriteLine(FormatXml(responseData));
            }
            Console.WriteLine();

            //
            // Release Token
            //
            // It is important to release the token for security reasons and to release 
            // any resources being retained for that token.
            Console.WriteLine("---Release Token---");
            try
            {
                result = ops.ReleaseToken();
                Console.WriteLine("Token Released.");
                Console.WriteLine("Result:" + result.ToString());
                Console.WriteLine();
            }
            catch (WebException wex)
            {
                Console.WriteLine("Unable to release token:");
                Console.WriteLine(wex.Message + " (" + wex.Status.ToString() + ")");
                Console.WriteLine();
            }
        }

        #region Transaction Push Helpers
        static private XmlDocument GenerateNewPermitXml(int permitNumber)
        {
            XmlDocument permitXmlDoc = new XmlDocument();

            permitXmlDoc.LoadXml(
                "<Permit>" +
                    "<PermitNumber>" + permitNumber.ToString() + "</PermitNumber>" +
                    "<PurchasedDate>" + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "</PurchasedDate>" +
                    "<ExpiryDate>" + DateTime.Now.AddHours(2).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "</ExpiryDate>" +
                    "<PermitAmount>2.00</PermitAmount>" +
                    //"<StallNumber>1</StallNumber>" +  // Optional field
                    //"<PlateNumber>clk123</PlateNumber>" +  // Optional field
                    "<RegionName>East Side</RegionName>" +  // Optional field
                                                            //"<RateName>$1 for 1hrs</RateName>" +  // Optional field
                    "<Payments>" +
                        "<Payment Type=\"CreditCard\">" +
                            "<Amount>2.00</Amount>" +
                            "<CardType>VISA</CardType>" +
                            "<Last4DigitsOfCard>1234</Last4DigitsOfCard>" +
                            "<CardAuthorizationId>def456</CardAuthorizationId>" +
                            "<PaymentDate>" + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "</PaymentDate>" +
                        "</Payment>" +
                    "</Payments>" +
                "</Permit>");

            return permitXmlDoc;
        }

        static private XmlDocument GenerateExtendPermitXml(XmlDocument currentPermit)
        {
            // The permit will be extended to reflect a customer extending their permit by 2 hours. The rate for the next 2 hours is $3
            // Update:
            //   - the expiry date (plus 2 hours)
            //   - total permit amount $5 (plus $3)
            //   - payment: new payment of $3

            XmlDocument updatePermit = (XmlDocument)currentPermit.CloneNode(true);

            foreach (XmlNode node in updatePermit.DocumentElement.ChildNodes)
            {
                switch (node.Name.ToLower())
                {
                    case "permitnumber":
                    case "purchaseddate":
                        // do nothing, required fields
                        break;
                    case "expirydate":
                        // add 2 hours to the permit
                        node.InnerText = DateTime.ParseExact(node.InnerText, "yyyy-MM-ddTHH:mm:ss", null).AddHours(2).ToString("yyyy-MM-ddTHH:mm:ss");
                        break;
                    case "permitamount":
                        // add $1 to the cost
                        node.InnerText = ((float.Parse(node.InnerText)) + 3.00).ToString();
                        break;
                    case "payments":
                        node.AppendChild(node.ChildNodes[0].CloneNode(true));
                        // add a $1 card payment to the transaction
                        // Note: Create the new payment to relfect extension. It will create another payment entry in the system.
                        //       Refer to the API documentation for different payment examples.
                        node.ChildNodes[1].SelectSingleNode("Amount").InnerText = "3.00";
                        node.ChildNodes[1].SelectSingleNode("CardAuthorizationId").InnerText = "ghi789";
                        node.ChildNodes[1].SelectSingleNode("PaymentDate").InnerText =
                            DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss");
                        break;
                    default:
                        // remove all other tags (if supplied)
                        // currentPermit.DocumentElement.RemoveChild(node);
                        break;
                }
            }

            return updatePermit;
        }

        #endregion

        #region General Helpers
        static private string FormatXml(XmlDocument xd)
        {
            //will hold formatted xml
            StringBuilder sb = new StringBuilder();

            //pumps the formatted xml into the StringBuilder above
            StringWriter sw = new StringWriter(sb);

            //does the formatting
            XmlTextWriter xtw = null;

            try
            {
                //point the xtw at the StringWriter
                xtw = new XmlTextWriter(sw);

                //we want the output formatted
                xtw.Formatting = Formatting.Indented;

                //get the dom to dump its contents into the xtw 
                xd.WriteTo(xtw);
            }
            finally
            {
                //clean up even if error
                if (xtw != null)
                    xtw.Close();
            }

            //return the formatted xml
            return sb.ToString();
        }

        static private string FormatXml(string sUnformattedXml)
        {
            //load unformatted xml into a dom
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(sUnformattedXml);

            return FormatXml(xd);
        }
        #endregion

        #region Coupon Push Helpers
        static private XmlDocument GetNewCouponXml(string couponCode)
        {
            XmlDocument couponXml = new XmlDocument();

            couponXml.LoadXml(
                "<Coupon>" +
                   "<CouponCode>" + couponCode + "</CouponCode>" +
                   "<Description>Test Coupon</Description>" +
                   "<DiscountType>Amount</DiscountType>" +
                   "<DiscountAmt>5.00</DiscountAmt>" +
                   // "<DiscountPercent></DiscountPercent>" +
                   // "<StartDate>" + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "</StartDate>" +
                   // "<EndDate></EndDate>" +
                   "<Uses>5</Uses>" +
                   // "<Location>Lot 22</Location>" +
                   "<OperatingMode>PND/PBL</OperatingMode>" +
                   "<SpaceRange></SpaceRange>" + // required field, should be optional (EMS-3891)
                "</Coupon>");

            return couponXml;
        }

        static private XmlDocument GetUpdateCouponXml(string couponCode)
        {
            XmlDocument couponXml = new XmlDocument();

            couponXml.LoadXml(
                "<Coupon>" +
                   "<CouponCode>" + couponCode + "</CouponCode>" +
                   "<Description>Test Coupon</Description>" +
                   "<DiscountType>Percent</DiscountType>" +
                   // "<DiscountAmt>5.00</DiscountAmt>" +
                   "<DiscountPercent>25</DiscountPercent>" +
                   // "<StartDate>" + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "</StartDate>" +
                   // "<EndDate></EndDate>" +
                   // "<Uses>5</Uses>" +
                   // "<Location>Lot 22</Location>" +
                   "<OperatingMode>PND/PBL</OperatingMode>" +
                   "<SpaceRange></SpaceRange>" + // rqeuired field, should be optional (EMS-3891)
                "</Coupon>");

            return couponXml;
        }

        static private XmlDocument GetNewCouponsXml(string[] couponCodes)
        {
            XmlDocument couponXml = new XmlDocument();

            string xmlText = "<Coupons>";
            foreach (string couponCode in couponCodes)
            {
                xmlText +=
                    "<Coupon>" +
                       "<CouponCode>" + couponCode + "</CouponCode>" +
                       "<Description></Description>" + // required field, should be optional (EMS-3891)
                       "<DiscountType>Amount</DiscountType>" +
                       "<DiscountAmt>5.00</DiscountAmt>" +
                       // "<DiscountPercent></DiscountPercent>" +
                       "<StartDate>" + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "</StartDate>" +
                       // "<EndDate></EndDate>" +
                       "<Uses>5</Uses>" +
                       // "<Location>Lot 22</Location>" +
                       "<OperatingMode>PND/PBL</OperatingMode>" +
                       "<SpaceRange></SpaceRange>" + // required field, should be optional (EMS-3891)
                    "</Coupon>";
            }
            xmlText += "</Coupons>";

            couponXml.LoadXml(xmlText);

            return couponXml;
        }

        static private string GetCouponList(string[] couponCodes)
        {
            string result = "";
            foreach (string couponCode in couponCodes)
            {
                result += couponCode + ",";
            }

            return result.Trim(',');
        }

        static private XmlDocument GetDeleteCouponsXml(string[] couponCodes)
        {
            XmlDocument couponXml = new XmlDocument();

            string xmlText = "<CouponCodes>";
            foreach (string couponCode in couponCodes)
            {
                xmlText += "<CouponCode>" + couponCode + "</CouponCode>";
            }
            xmlText += "</CouponCodes>";

            couponXml.LoadXml(xmlText);

            return couponXml;
        }
        #endregion

    }
}
