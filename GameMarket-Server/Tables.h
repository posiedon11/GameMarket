#pragma once
#ifndef TABLES_LOCK
#define TABLES_LOCK

#include<string>
#include"Tools.h"


namespace tables_namespace
{

	enum class CRUD {
		Insert,
		Update,
		Delete,
		Select,
		UpSert
	};
	enum class Tables {
		//xbox
		XboxUserProfiles,
		XboxGameTitles,
		XboxGameBundles,
		XboxProductIds,
		XboxTitleDetails,
		XboxMarketDetails,
		XboxGameGenres,
		XboxGroupData,
		//steam
		SteamAppIDs,
		SteamAppDetails,
		SteamAppDevelopers,
		SteamAppPublishers,
		SteamPackages,
		SteamPackageDetails,
		SteamPackageIDs,
		SteamAppPlatforms,
		SteamAppGenres

		//other
	};

	struct TableData
	{
		virtual ~TableData() = default;
	};
	struct genericXboxData : public TableData
	{
		std::string ItemType;
		std::string productID;
	};

	struct XboxGameMarketData : public TableData
	{
		std::vector<std::string> productIDs;
		std::string productID;
		std::string devName, pubName, productTitle, productDesc;
		std::string releaseDate;
		std::string primarySku;
		std::string startDate, endDate;

		float listPrice, MSRP;

		void outputData();
	};
	struct XboxProfileData : public TableData {
		string xuid;
		
	};
	struct XboxTitleDetailsData : public TableData
	{
		string titleID, productID;
		vector<string> bundleIDs;

		void outputData();
	};

	struct XboxGameTitleData : public TableData
	{
		string titleID, modernTitleID, titleName, displayImage;
		vector<string> devices;
		bool isGamePass;


		void outputData();
	};

	struct XboxUpdateScannedData : public TableData
	{
		string ID;
	};
	struct XboxProductGroupData: public TableData
	{
		string groupID, groupName, productID, titleID;
	};


	struct SteamAPPListData : public TableData
	{
		string name;
		uint32_t appid;

		void outputData();
	};
	struct SteamAppDetailsData : public TableData
	{
		string name, type;
		vector<uint32_t> dlc, packages;
		vector<string> developters, publishers, platforms, genres;
		uint32_t appid;
	};


	std::string toString(Tables collection);
}

#endif // !COLLECTIONS_LOCK