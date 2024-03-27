#include"Tables.h"


namespace tables_namespace
{
	std::string toString(Tables collection)
	{
		switch (collection)
		{
			//xbox tables
		case Tables::XboxGameGenres: return "GameGenres";
		case Tables::XboxGameTitles: return "GameTitles";
		case Tables::XboxUserProfiles: return "UserProfiles";
		case Tables::XboxProductIds: return "ProductIds";
		case Tables::XboxGameBundles: return "GameBundles";
		case Tables::XboxTitleDetails: return "TitleDetails";
		case Tables::XboxMarketDetails: return "MarketDetails";
		case Tables::XboxGroupData: return "GroupData";
		
			//steam tables
		case Tables::SteamAppIDs: return "APPIds";
		case Tables::SteamAppDetails: return "AppDetails";
		case Tables::SteamAppPublishers: return "AppPublishers";
		case Tables::SteamAppDevelopers: return "AppDevelopers";
		case Tables::SteamAppGenres: return "AppGenres";
		case Tables::SteamPackageIDs:return "PackageIDs";
		case Tables::SteamPackageDetails: return "PackageDetails";
		case Tables::SteamPackages: return "Packages";
		case Tables::SteamAppPlatforms: return "AppPlatforms";

		default: return "";
		}
	}
	void XboxGameMarketData::outputData()
	{
		std::cout << "Product IDs:  ";
		for (std::string id : productIDs)
		{
			std::cout << id << " ";
		}
		std::cout << std::endl;
		std::cout << "ProductID: " << productID << std::endl;
		std::cout << "Dev Name: " << devName << std::endl << "Pub Name: " << pubName << std::endl << "Title: "
			<< productTitle << std::endl << "productDesc: " << std::endl << productDesc << std::endl;

		std::cout << "ListPrice: " << std::to_string(listPrice) << std::endl << "MSRP:  " << std::to_string(MSRP) << std::endl;
	}

	void XboxGameTitleData::outputData()
	{
		cout << "Title: " << titleName << endl;
		cout << "TitleId: " << titleID << endl;
		cout << "ModernTitleId: " << modernTitleID << endl;
		cout << "Game Pass: " << isGamePass << endl;
		cout << "Devices: ";
		for (string device : devices)
		{
			cout << device << ", ";
		}
		cout << endl <<endl;
	}
	void XboxTitleDetailsData::outputData()
	{
		cout << "titleID: " << titleID << endl;
		cout << "productID: " << productID << endl;

		cout << "Bundle IDs: ";
		for (string bundle : bundleIDs)
		{
			cout << bundle << "  ";
		}
		cout << endl << endl;
	}
	void SteamAPPListData::outputData()
	{
		cout << "APPid: " << appid << endl;
		cout << "Name: " << name << endl <<endl;
	}
}