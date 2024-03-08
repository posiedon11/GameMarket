#pragma once
#ifndef APIMANAGER_LOCK
#define APIMANAGER_LOCK
#include"Tools.h"
#include"DataBaseManager.h"

using namespace DataBase_NameSpace;

namespace apimanager_namespace
{
	class APIManager
	{
	public:
		virtual enum class APICalls {};
		virtual std::string to_string(APICalls call) = 0;
		virtual std::string getURL(APICalls call, std::string paramater = "") = 0;

	private:
		std::atomic<bool> running;
		std::thread mainThread;
		DataBaseManager& dbManager;
		Settings& settings;


	public:
		void start();
		void stop();
		virtual ~APIManager();
		APIManager(DataBaseManager &dbManager, Settings &settings);
	private:
		void run();
		virtual void startCalls() = 0;
	};
}
#endif // !APIMANAGER_LOCK
