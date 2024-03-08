#pragma once
#ifndef TABLES_LOCK
#define TABLES_LOCK

#include<string>
#include"Tools.h"


namespace tables_namespace
{
	enum class Tables {
		XboxUserProfiles,
		XboxGameTitles,
		XboxGameBundles,
		XboxGameDetails,
		XboxTitleDetails,
		XboxMarketDetails,
		XboxGameGenres,
		SteamGames,
		SteamGameGenres
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


	std::string toString(Tables collection);
}

#endif // !COLLECTIONS_LOCK