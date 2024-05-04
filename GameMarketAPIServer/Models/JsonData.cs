using GameMarketAPIServer.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using static GameMarketAPIServer.Models.DataBaseSchemas;

namespace GameMarketAPIServer.Models
{

    public interface IJsonData
    {
        void output(int outputNestedDepth = 0);
    }
    public abstract class JsonData : IJsonData
    {
        protected ILogger logger;

        protected JsonData(ILogger logger)
        {
            this.logger = logger;
        }
        public abstract void output(int outputNestedDepth = 0);

        public void InitializeJsonData(ILogger logger)
        {
            Queue<JsonData> queue = new Queue<JsonData>();
            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                current.logger = logger;  // Set the logger

                foreach (var property in current.GetType().GetProperties())
                {
                    var value = property.GetValue(current);
                    if (value is JsonData child)
                    {
                        queue.Enqueue(child);
                    }
                    else if (value is IEnumerable<JsonData> children)
                    {
                        foreach (var child1 in children)
                        {
                            queue.Enqueue(child1);
                        }
                    }
                }
            }
        }
    }


    //For Xbox player history:
    #region PlayerHistory
    public class XboxTitleHistory : JsonData
    {
        public XboxTitleHistory(ILogger logger) : base(logger) { }
        public string xuid { get; set; }
        public List<XboxTitle> titles { get; set; }
        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"\nPlayer Title History: ");
            logger.LogDebug($"XUID: {xuid}");

            if (outputNestedDepth > 0)
            {
                logger.LogDebug($"Xbox Titles: ");
                foreach (var title in titles)
                {
                    title.output(outputNestedDepth - 1);
                }

            }
        }
    }
    public class XboxTitle : JsonData
    {
        public XboxTitle(ILogger logger) : base(logger) { }
        public string titleId { get; set; }
        public string modernTitleId { get; set; }
        public string name { get; set; }
        public string displayImage { get; set; }
        public bool isBundle { get; set; }
        public List<string> devices { get; set; }
        public XboxGamePass gamePass { get; set; }


        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"\nXbox Title:");
            logger.LogDebug($"TitleID {titleId}");
            logger.LogDebug($"Modern TitleID: {modernTitleId}");
            logger.LogDebug($"Name:  {name}");
            logger.LogDebug($"Display Image:  {displayImage}");
            logger.LogDebug($"Is Bundle:  {isBundle}");

            Console.Write($"Devices: ");
            var deviceString = "\t";
            foreach (var device in devices)
            {
                deviceString += $"{device},";
            }
            logger.LogDebug($"Devices: \n{deviceString}\n");
            if (outputNestedDepth > 0)
            {
                gamePass.output(outputNestedDepth - 1);
            }
        }
    }
    public class XboxGamePass : JsonData
    {
        public XboxGamePass(ILogger logger) : base(logger) { }
        public bool isGamePass { get; set; }
        public override void output(int outputNestedDepth = 0)
        {

            logger.LogDebug($"Is Game Pass: {isGamePass}");
        }



    }
    #endregion


    //For Xbox Generic
    #region Generic
    public class GenericData : JsonData
    {
        public GenericData(ILogger logger) : base(logger) { }
        public List<Item> items { get; set; }
        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"Product: ");
            if (outputNestedDepth > 0)
            {
                foreach (var item in items)
                {
                    item.output(outputNestedDepth - 1);
                }
            }
        }
    }
    public class Item : JsonData
    {
        public Item(ILogger logger) : base(logger) { }
        public string Id { get; set; }
        public string ItemType { get; set; }
        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"Item: ");
            logger.LogDebug($"ID: {Id}");
            logger.LogDebug($"Item Type: {ItemType}");
            if (outputNestedDepth > 0)
            {
            }


        }
    }

    #endregion
    //For Xbox Title Details and Market Details
    #region MarketProperties
    public class XboxMarketDetails : JsonData
    {
        public XboxMarketDetails(ILogger logger) : base(logger) { }
        public List<string> bigIds { get; set; }
        public List<XboxProduct> products { get; set; }


        public override void output(int outputNestedDepth = 0)
        {
            //Console.Write($"BigIDs: ");
            string bigIdString = "\t";
            foreach (var bigID in bigIds)
            {
                bigIdString += $"{bigID}, ";
            }
            logger.LogDebug($"\nBigIds:\n{bigIdString}");
            if (outputNestedDepth > 0)
            {
                logger.LogInformation($"Products: \n\n");
                foreach (var product in products)
                {
                    product.output(outputNestedDepth - 1);
                    logger.LogDebug($"\n\n\n");
                }
            }
        }
    }
    public class XboxProduct : JsonData
    {
        public XboxProduct(ILogger logger) : base(logger) { }
        public List<XboxLocalizedProperties> localizedProperties { get; set; }

        public List<XboxMarketProperties> marketProperties { get; set; }

        public string productId { get; set; }
        public XboxProductProperties properties { get; set; }
        public List<XboxAlternateId> alternateIds { get; set; }

        public bool IsSandboxedProduct { get; set; }
        public string SandboxId { get; set; }
        public List<XboxDisplaySkuAvailability> displaySkuAvailabilities { get; set; }


        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"Product: ");
            if (outputNestedDepth > 0)
            {
                logger.LogDebug($"Localized Properties: \n");
                foreach (var localizedProperty in localizedProperties)
                {
                    localizedProperty.output(outputNestedDepth - 1);
                }
                logger.LogDebug($"Market Properties: \n");
                foreach (var marketProperty in marketProperties)
                {
                    marketProperty.output(outputNestedDepth - 1);
                }

                properties.output(outputNestedDepth - 1);

                logger.LogDebug($"Alternate IDs: \n");
                foreach (var alternateId in alternateIds)
                {
                    alternateId.output(outputNestedDepth - 1);
                }
                logger.LogDebug($"Display Sku Availabilites: ");
                foreach (var displaySkuAvailability in displaySkuAvailabilities)
                {
                    displaySkuAvailability.output(outputNestedDepth - 1);
                }

            }
            logger.LogDebug($"Product ID: {productId}");
            logger.LogDebug($"SandBoxID: {SandboxId}");
            logger.LogDebug($"Is SandBoxed Product:  {IsSandboxedProduct}");
        }
    }

    public class XboxLocalizedProperties : JsonData
    {
        public XboxLocalizedProperties(ILogger logger) : base(logger) { }
        public string developerName { get; set; }
        public string publisherName { get; set; }
        public string productDescription { get; set; }
        public string productTitle { get; set; }
        public string shorttitle { get; set; }

        public List<XboxImages> Images { get; set; }
        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"Localized Property:");

            if (outputNestedDepth > 0)
            {

            }
            logger.LogDebug($"Product Title:  {productTitle}");
            logger.LogDebug($"Short Title:  {shorttitle}");
            logger.LogDebug($"Developer Name:  {developerName}");
            logger.LogDebug($"Publisher Name:  {publisherName}");
            logger.LogDebug($"Product Description:  {productDescription}\n");
        }

    }

    public class XboxImages : JsonData
    {
        public XboxImages(ILogger logger) : base(logger) { }
        public string imagePurpose { get; set; }
        public string uri { get; set; }
        public int height { get; set; }
        public int width { get; set; }

        public override void output(int outputNestedDepth = 0)
        {
            throw new NotImplementedException();
        }
    }
    public class XboxMarketProperties : JsonData
    {
        public XboxMarketProperties(ILogger logger) : base(logger) { }
        public List<XboxRelatedProduct> relatedProducts { get; set; }
        public string OriginalReleaseDate { get; set; }

        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"Market Property: ");
            if (outputNestedDepth > 0)
            {
                logger.LogDebug($"Related Products: \n");
                foreach (var relatedProduct in relatedProducts)
                {
                    relatedProduct.output(outputNestedDepth - 1);
                }
            }
        }
    }
    //bundles
    public class XboxRelatedProduct : JsonData
    {
        public XboxRelatedProduct(ILogger logger) : base(logger) { }
        public string relatedProductId { get; set; }
        public string relationshipType { get; set; }

        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"\nRelated Product: ");
            if (outputNestedDepth > 0)
            {

            }
            logger.LogDebug($"Related Product Id:  {relatedProductId}");
            logger.LogDebug($"Relationship Type:  {relationshipType}\n");
        }
    }

    public class XboxProductProperties : JsonData
    {
        public XboxProductProperties(ILogger logger) : base(logger) { }
        public bool isDemo { get; set; }

        public bool isAccessible { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }

        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"Product Properties: ");
            if (outputNestedDepth > 0)
            {

            }
            logger.LogDebug($"Is Demo:  {isDemo}");
            logger.LogDebug($"Is Accessible:  {isAccessible}");
            logger.LogDebug($"Product Group ID:  {ProductGroupId}");
            logger.LogDebug($"Product Group Name:  {ProductGroupName}");
            logger.LogDebug($"\n");
        }
    }
    public class XboxAlternateId : JsonData
    {
        public XboxAlternateId(ILogger logger) : base(logger) { }
        public string idType { get; set; }
        public string value { get; set; }
        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"Alternalte ID: ");
            if (outputNestedDepth > 0)
            {

            }
            logger.LogDebug($"ID Type {idType}");
            logger.LogDebug($"Value:  {value}");
        }
    }

    public class XboxDisplaySkuAvailability : JsonData
    {
        public XboxDisplaySkuAvailability(ILogger logger) : base(logger) { }
        public XboxSku sku { get; set; }
        public List<XboxAvailability> availabilities { get; set; }


        public override void output(int outputNestedDepth = 0)
        {

            if (outputNestedDepth > 0)
            {
                sku.output(outputNestedDepth - 1);
                logger.LogDebug($"Availabiliteis: \n");
                foreach (var availability in availabilities)
                {
                    availability.output(outputNestedDepth - 1);
                }
                logger.LogDebug($"\n");

            }
        }
    }

    public class XboxSku : JsonData
    {
        public XboxSku(ILogger logger) : base(logger) { }
        public string productID { get; set; }
        public XboxSkuProperties properties { get; set; }

        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"SKU: \n");
            if (outputNestedDepth > 0)
            {
                properties.output(outputNestedDepth - 1);
            }
            logger.LogDebug($"Sku Product ID:  {productID}");
        }
    }
    public class XboxSkuProperties : JsonData
    {
        public XboxSkuProperties(ILogger logger) : base(logger) { }
        public override void output(int outputNestedDepth = 0)
        {

            if (outputNestedDepth > 0)
            {

            }
        }
    }

    public class XboxAvailability : JsonData
    {
        public XboxAvailability(ILogger logger) : base(logger) { }
        public List<string> actions { get; set; }
        public XboxConditions conditions { get; set; }

        public XboxOrderManagementData orderManagementData { get; set; }
        public bool remediationRequired { get; set; }

        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"Availability:");
            Console.Write($"Actions :  ");
            foreach (var action in actions)
            {
                Console.Write(action + "  ");
            }
            if (outputNestedDepth > 0)
            {
                conditions.output(outputNestedDepth - 1);
                orderManagementData.output(outputNestedDepth - 1);
            }


            logger.LogDebug($"\nRemediation Required:  {remediationRequired}\n");
        }

    }

    public class XboxConditions : JsonData
    {
        public XboxConditions(ILogger logger) : base(logger) { }
        public string endDate { get; set; }
        public string startDate { get; set; }
        public XboxClientConditions clientConditions { get; set; }

        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"Conditions: \n");
            if (outputNestedDepth > 0)
            {
                logger.LogDebug($"Platforms. ");
            }

            logger.LogDebug($"End Date:  {endDate}");
            logger.LogDebug($"Start Date:  {startDate}");
        }
    }
    public class XboxClientConditions : JsonData
    {
        public XboxClientConditions(ILogger logger) : base(logger) { }
        public List<XboxAllowedPlatforms> allowedPlatforms { get; set; }
        public override void output(int outputNestedDepth = 0) { }
    }
    public class XboxAllowedPlatforms : JsonData
    {
        public XboxAllowedPlatforms(ILogger logger) : base(logger) { }
        public string platformName { get; set; }

        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"Conditions: \n");
            if (outputNestedDepth > 0)
            {

            }

            logger.LogDebug($"Platform Name:  {platformName}");
        }

    }
    public class XboxOrderManagementData : JsonData
    {
        public XboxOrderManagementData(ILogger logger) : base(logger) { }
        public XboxPrice price { get; set; }


        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"Order Management Data: \n");
            if (outputNestedDepth > 0)
            {
                price.output(outputNestedDepth - 1);
            }
        }
    }
    public class XboxPrice : JsonData
    {
        public XboxPrice(ILogger logger) : base(logger) { }
        public string currencyCode { get; set; }
        public double listPrice { get; set; }
        public double msrp { get; set; }

        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"Price: \n");
            if (outputNestedDepth > 0)
            {

            }
            logger.LogDebug($"List Price:  {listPrice}");
            logger.LogDebug($"MSRP:  {msrp}");
            logger.LogDebug($"Currency Code:  {currencyCode}");
        }
    }
    #endregion


    //For Steam App list
    #region SteamAppList


    public class SteamAppListMain : JsonData
    {
        public SteamAppListMain(ILogger logger) : base(logger) { }
        //v2
        public SteamAppList appList { get; set; }
        //v1
        public SteamAppList response { get; set; }



        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"Get Steam App List: ");
            if (outputNestedDepth > 0)
            {
                appList.output(outputNestedDepth - 1);
            }
        }
    }
    public class SteamAppList : JsonData
    {
        public SteamAppList(ILogger logger) : base(logger) { }
        public List<SteamApp> apps { get; set; }

        //For the v1 version
        public bool have_more_results { get; set; }
        public UInt32 last_appid { get; set; }
        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"Product: ");
            if (outputNestedDepth > 0)
            {
                foreach (var app in apps)
                {
                    app.output(outputNestedDepth - 1);
                }
            }
        }
    }
    public class SteamApp : JsonData
    {
        public SteamApp(ILogger logger) : base(logger) { }
        public UInt32 appID { get; set; }
        public string name { get; set; }

        public override void output(int outputNestedDepth = 0)
        {
            logger.LogDebug($"\nProduct: ");
            logger.LogDebug($"App ID:  {appID}");
            logger.LogDebug($"Name:  {name}");
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

        public HashSet<UInt32> dlc { get; set; }
        public string short_description { get; set; }

        public SteamFullGame fullgame { get; set; }

        public string header_image { get; set; }
        public string capsule_image { get; set; }
        public string capsule_imagev5 { get; set; }

        public HashSet<string> developers { get; set; }
        public HashSet<string> publishers { get; set; }

        public SteamPriceOverview price_overview { get; set; }
        public HashSet<UInt32> packages { get; set; }

        public HashSet<SteamGenre> genres { get; set; }
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



    public class GameMarketTitledata
    {
        Int32 gameID { get; set; }
        string gameTitle { get; set; }

        SortedSet<string> developers { get; set; }
        SortedSet<string> publishers { get; set; }

    }
    public class GameMarketPlatformIDsData : JsonData
    {

        public SortedSet<string> xboxIds { get; set; }
        public SortedSet<string> steamIds { get; set; }
        public GameMarketPlatformIDsData(ILogger logger) : base(logger) { }
        public override void output(int outputNestedDepth = 0)
        {
            throw new NotImplementedException();
        }
    }


    public class GameMarketDTO
    {
        public string GameTitle { get; set; }
        public UInt32 GameID { get; set; }
        public List<string> Developers { get; set; }
        public List<string> Publishers { get; set; }
        public ICollection<XboxSchema.MarketDetailTable> XboxMarketDetails { get; set; }

        public ICollection<SteamSchema.AppDetailsTable> SteamAppDetails { get; set; }
    }

    public class GameMarketTitleDTO
    {
        public string GameTitle { get; set; }
        public UInt32 GameID { get; set; }
        public List<string> Developers { get; set; }
        public List<string> Publishers { get; set; }
        public ICollection<XboxDetailsDTO> XboxDetails { get; set; }

        public ICollection<SteamDetailsDTO> SteamDetails { get; set; }
    }

    public class GameMarketListDTO
    {
        public string GameTitle { get; set; }
        public UInt32 GameID { get; set; }
        public ICollection<XboxPriceDTO> XboxPriceDetails { get; set;}
        public ICollection<SteamPriceDTO> SteamPriceDetails { get; set; }

    }
    public class XboxPriceDTO
    {
        public string ProductID { get; set; }
        public string CurrencyCode { get; set; }
        public double? ListPrice { get; set; }
        public double? MSRP { get; set; }

        public string? imageURI { get; set; }
        public bool Purchasable { get; set; }

        public ICollection<string> Platforms { get; set; } = new List<string>();

    }

    public class SteamPriceDTO
    {
        public UInt32 AppID { get; set; }
        public double? MSRP { get; set; }
        public double? ListPrice { get; set; }
        public string? imageURI { get; set; }
        public bool isFree { get; set; }

        public ICollection<string> Platforms { get; set; } = new List<string>();

    }


    public class XboxDetailsDTO
    {
        public string storeURL { get; set; }
        public XboxPriceDTO PriceDetails { get; set; }
        public ICollection<string> Bundles { get; set; }
    }

    public class SteamDetailsDTO
    {
        public string storeURL { get; set; }
        public SteamPriceDTO PriceDetails { get; set; }
        public ICollection<UInt32> packeges { get; set; }
    }

}
