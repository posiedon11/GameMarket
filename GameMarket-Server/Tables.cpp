#include"Tables.h"


namespace tables_namespace
{
	std::string toString(Tables collection)
	{
		switch (collection)
		{
		case Tables::XboxGameGenres: return "XboxGameGenres";
		case Tables::XboxGameTitles: return "XboxGameTitles";
		case Tables::XboxUserProfiles: return "XboxUserProfiles";
		case Tables::XboxGameDetails: return "XboxGameDetails";
		case Tables::XboxGameBundles: return "XboxGameBundles";
		case Tables::XboxTitleDetails: return "XboxTitleDetails";
		case Tables::XboxMarketDetails: return "XboxMarketDetails";
		case Tables::SteamGames: return "SteamGames";
		case Tables::SteamGameGenres: return "SteamGameGenres";

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
}