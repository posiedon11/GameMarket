﻿namespace GameMarketAPIServer.Models
{



    //For Xbox player history:
    #region PlayerHistory
    public class XboxTitleHistory
    {
        public string xuid { get; set; }
        public List<XboxTitle> titles { get; set; }
        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Player Title History: ");
            Console.WriteLine("XUID: " + xuid);

            if (outputNestedDepth > 0)
            {
                Console.WriteLine("Titles: ");
                foreach (var title in titles)
                {
                    title.output(outputNestedDepth-1);
                    Console.WriteLine();
                }

            }
        }
    }
    public class XboxTitle
    {

        public string titleId { get; set; }
        public string modernTitleId { get; set; }
        public string name { get; set; }
        public string displayImage { get; set; }
        public bool isBundle { get; set; }
        public List<string> devices { get; set; }
        public XboxGamePass gamePass { get; set; }


        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("TitleID: " + titleId);
            Console.WriteLine("Modern TitleID: " + modernTitleId);
            Console.WriteLine("Name: " + name);
            Console.WriteLine("Display Image: " + displayImage);
            Console.WriteLine("Is Bundle: " + isBundle);

            Console.Write("Devices: ");
            foreach(var device in devices)
            {
                Console.Write(device + "  ");
            }
            Console.WriteLine("\n");
            if (outputNestedDepth > 0)
            {
                gamePass.output(outputNestedDepth-1);
            }
            Console.WriteLine();
        }
    }
    public class XboxGamePass
    {
        public bool isGamePass { get; set; }
        public void output(int outputNestedDepth = 0)
        {

            Console.WriteLine("Is Game Pass: " + isGamePass);
        }



    }
    #endregion


    //For Xbox Generic
    #region Generic
    public class GenericData
    {
        public List<Item> items { get; set; }
        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Product: ");
            if (outputNestedDepth > 0)
            {
                foreach (var item in items)
                {
                    item.output(outputNestedDepth - 1);
                }
            }
        }
    }
    public class Item
    {
        public string Id { get; set; }
        public string ItemType { get; set; }
        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Item: ");
            Console.WriteLine("ID: " + Id);
            Console.WriteLine("Item Type: " + ItemType);
            if (outputNestedDepth > 0)
            {
            }


        }
    }

    #endregion
    //For Xbox Title Details and Market Details
    #region MarketProperties
    public class XboxMarketDetails
    {
        public List<string> bigIds { get; set; }
        public List<XboxProduct> products { get; set; }

        public void output(int outputNestedDepth = 0)
        {
            Console.Write("BigIDs: ");
            foreach (var bigID in bigIds)
            {
                Console.Write(bigID + "  ");
            }
            Console.WriteLine("\n");
            if (outputNestedDepth > 0)
            {
                Console.WriteLine("Products: \n\n");
                foreach (var product in products)
                {
                    product.output(outputNestedDepth - 1);
                    Console.WriteLine("\n\n\n");
                }
            }
        }
    }
    public class XboxProduct
    {
        public List<XboxLocalizedProperties> localizedProperties { get; set; }

        public List<XboxMarketProperties> marketProperties { get; set; }

        public string productId { get; set; }
        public XboxProductProperties properties { get; set; }
        public List<XboxAlternateId> alternateIds { get; set; }

        public bool IsSandboxedProduct { get; set; }
        public string SandboxId { get; set; }
        public List<XboxDisplaySkuAvailability> displaySkuAvailabilities { get; set; }


        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Product: ");
            if (outputNestedDepth > 0)
            {
                Console.WriteLine("Localized Properties: \n");
                foreach(var localizedProperty in localizedProperties)
                {
                    localizedProperty.output(outputNestedDepth-1);
                }
                Console.WriteLine("Market Properties: \n");
                foreach(var marketProperty in marketProperties)
                {
                    marketProperty.output(outputNestedDepth-1);
                }

                properties.output(outputNestedDepth-1);

                Console.WriteLine("Alternate IDs: \n");
                foreach(var alternateId in alternateIds)
                {
                    alternateId.output(outputNestedDepth-1);
                }
                Console.WriteLine("Display Sku Availabilites: ");
                foreach (var displaySkuAvailability in displaySkuAvailabilities)
                {
                    displaySkuAvailability.output(outputNestedDepth-1);
                }

            }
            Console.WriteLine("Product ID: " + productId);
            Console.WriteLine("SandBoxID: " + SandboxId);
            Console.WriteLine("Is SandBoxed Product: " + IsSandboxedProduct);
            Console.WriteLine();
        }
    }

    public class XboxLocalizedProperties
    {
        public string developerName { get; set; }
        public string publisherName { get; set; }
        public string productDescription { get; set; }
        public string productTitle { get; set; }
        public string shorttitle { get; set; }

        public List<XboxImages> Images { get; set; }
        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Localized Property:");

            if (outputNestedDepth > 0)
            {

            }
            Console.WriteLine("Product Title: " + productTitle);
            Console.WriteLine("Short Title: " + shorttitle);
            Console.WriteLine("Developer Name: " + developerName);
            Console.WriteLine("Publisher Name: " + publisherName);
            Console.WriteLine("Product Description: " + productDescription);
            Console.WriteLine();
        }

    }

    public class XboxImages
    {
        public string imagePurpose { get; set; }
        public string uri { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }
    public class XboxMarketProperties
    {
        public List<XboxRelatedProduct> relatedProducts { get; set; }
        public string OriginalReleaseDate { get; set; }

        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Market Property: ");
            if (outputNestedDepth > 0)
            {
                Console.WriteLine("Related Products: \n");
                foreach (var relatedProduct in relatedProducts)
                {
                    relatedProduct.output(outputNestedDepth-1);
                }
            }
        }
    }
    //bundles
    public class XboxRelatedProduct
    {
        public string relatedProductId { get; set; }
        public string relationshipType { get; set; }

        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Related Product: ");
            if (outputNestedDepth > 0)
            {

            }
            Console.WriteLine("Related Product Id: " + relatedProductId);
            Console.WriteLine("Relationship Type: " + relationshipType);
            Console.WriteLine();
        }
    }

    public class XboxProductProperties
    {
        public bool isDemo { get; set; }

        public bool isAccessible { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductGroupName  { get; set; }

        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Product Properties: ");
            if (outputNestedDepth > 0)
            {

            }
            Console.WriteLine("Is Demo: " + isDemo);
            Console.WriteLine("Is Accessible: " + isAccessible);
            Console.WriteLine("Product Group ID: " + ProductGroupId);
            Console.WriteLine("Product Group Name: " + ProductGroupName);
            Console.WriteLine("\n");
        }
    }
    public class XboxAlternateId
    {
        public string idType { get; set; }
        public string value { get; set; }
        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Alternalte ID: ");
            if (outputNestedDepth > 0)
            {

            }
            Console.WriteLine("ID Type" + idType);
            Console.WriteLine("Value: " + value);
            Console.WriteLine();
        }
    }

    public class XboxDisplaySkuAvailability
    {
        public XboxSku sku { get; set; }
        public List<XboxAvailability> availabilities { get; set; }


        public void output(int outputNestedDepth = 0)
        {

            if (outputNestedDepth > 0)
            {
                sku.output(outputNestedDepth-1);
                Console.WriteLine("Availabiliteis: \n");
                foreach (var availability in availabilities)
                {
                    availability.output(outputNestedDepth-1);
                }
                Console.WriteLine();

            }
        }
    }

    public class XboxSku
    {
        public string productID { get; set; }
        public XboxSkuProperties properties { get; set; }

        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("SKU: \n");
            if (outputNestedDepth > 0)
            {
                properties.output(outputNestedDepth - 1);
            }
            Console.WriteLine("Sku Product ID: " + productID);
        }
    }
    public class XboxSkuProperties
    {
        public void output(int outputNestedDepth = 0)
        {

            if (outputNestedDepth > 0)
            {

            }
        }
    }

    public class XboxAvailability
    {
        public List<string> actions { get; set; }
        public XboxConditions conditions { get; set; }

        public XboxOrderManagementData orderManagementData { get; set; }
        public bool remediationRequired { get; set; }

        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Availability:");
            Console.Write("Actions :  ");
            foreach (var action in actions)
            {
                Console.Write(action + "  ");
            }
            if (outputNestedDepth > 0)
            {
                conditions.output(outputNestedDepth-1);
                orderManagementData.output(outputNestedDepth-1);
            }

            
            Console.WriteLine();
            Console.WriteLine("Remediation Required: " + remediationRequired);
            Console.WriteLine();
        }

    }

    public class XboxConditions
    {
        
        public string endDate { get; set; }
        public string startDate { get; set; }
        public XboxClientConditions clientConditions { get; set; }

        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Conditions: \n");
            if (outputNestedDepth > 0)
            {
                Console.WriteLine("Platforms. ");
            }

            Console.WriteLine("End Date: " + endDate);
            Console.WriteLine("Start Date: " + startDate);
        }
    }
    public class XboxClientConditions
    {
        public List<XboxAllowedPlatforms> allowedPlatforms { get; set; }
        public void output(int outputNestedDepth = 0) { }
    }
    public class XboxAllowedPlatforms
    {
        public string platformName { get; set; }

        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Conditions: \n");
            if (outputNestedDepth > 0)
            {

            }

            Console.WriteLine("Platform Name: " + platformName);
        }

    }
    public class XboxOrderManagementData
    {
        public XboxPrice price { get; set; }


        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Order Management Data: \n");
            if (outputNestedDepth > 0)
            {
                price.output(outputNestedDepth-1);
            }
        }
    }
    public class XboxPrice
    {
        public string currencyCode { get; set; }
        public double listPrice { get; set; }
        public double msrp { get; set; }

        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Price: \n");
            if (outputNestedDepth > 0)
            {

            }
            Console.WriteLine("List Price: " + listPrice);
            Console.WriteLine("MSRP: " + msrp);
            Console.WriteLine("Currency Code: " + currencyCode);
        }
    }
    #endregion


    //For Steam App list
    #region SteamAppList

    
    public class SteamAppListMain
    {
        public SteamAppList appList { get; set; }
        public SteamAppList response { get; set; }


        
        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Get Steam App List: ");
            if (outputNestedDepth > 0)
            {
                appList.output(outputNestedDepth-1);
            }
        }
    }
    public class SteamAppList
    {
        public List<SteamApp> apps { get; set; }

        //For the v1 version
        public bool have_more_results { get; set; }
        public UInt32 last_appid { get; set; }
        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("Product: ");
            if (outputNestedDepth > 0)
            {
                foreach (var app in apps)
                {
                    app.output(outputNestedDepth - 1);
                }
            }
        }
    }
    public class SteamApp
    {
        public UInt32 appid { get; set; }
        public string name { get; set; }

        public void output(int outputNestedDepth = 0)
        {
            Console.WriteLine("\nProduct: ");
            Console.WriteLine("App ID: " + appid);
            Console.WriteLine("Name: " +  name);
            if (outputNestedDepth > 0)
            {

            }
        }
    }
    #endregion


    #region SteamAppDetails
    public class SteamAppDetails
    {
        public bool success { get; set; }
        public SteamAppData data { get; set; }
    }
    public class SteamAppData
    { 
        public string type { get; set; }
        public string name { get; set; } 
        public UInt32 steam_appid { get; set; }
        public bool is_free { get; set; }

        public List<UInt32> dlc {  get; set; } 
        public string short_description { get; set; }

        public SteamFullGame fullgame { get; set; }

        public List<string> developers { get; set; }
        public List<string> publishers { get; set; }

        public SteamPriceOverview price_overview { get; set; }
        public List<UInt32> packages { get; set; }

        public List<SteamGenre> genres { get; set; }
        public SteamPlatforms platforms { get; set; }

        public SteamRelease release_date { get; set; }

    }
    public class SteamFullGame
    {
        public string appid { get; set; }
        public string name { get; set; }
    }

    public class SteamGenre
    {
        public string description { get; set; }
        public UInt32 id { get; set; }
    }
   public class SteamPriceOverview
    {
        public string currency { get; set; }
        public int initial { get; set; }
        public int final { get; set; }
        public int discount_percent { get; set; }


    }

    public class SteamPlatforms
    {
        public bool windows { get; set; }
        public bool mac { get; set; }
        public bool linux { get; set; }
    }

    public class SteamRelease
    {
        public bool coming_soon { get; set; }
        public string date { get; set; }
        
    }
    #endregion
}