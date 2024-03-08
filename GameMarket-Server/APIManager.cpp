#include"APIManager.h"


namespace apimanager_namespace
{
	void APIManager::start()
	{
		running = true;
		mainThread = std::thread(&APIManager::run, this);
	}
	void APIManager::stop()
	{
		running = false;
		if (mainThread.joinable())
			mainThread.join();

	}
	void APIManager::run()
	{
		while (running)
		{
			startCalls();
		}
	}

	APIManager::APIManager(DataBaseManager& dbManager, Settings& settings) :running(false), dbManager(dbManager), settings(settings) {};
	APIManager::~APIManager() { stop(); }
}