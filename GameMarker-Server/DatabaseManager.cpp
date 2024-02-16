
#include"DatabaseManager.h"



namespace dataBaseManager_namespace
{
	DataBaseManager* DataBaseManager::getInstance()
	{
		if (instance == NULL)
			instance = new DataBaseManager();
		return instance;
	}

	mysqlx::Schema* DataBaseManager::getSchema()
	{
		if (schema != nullptr)
			return schema;
	}
	void DataBaseManager::connectToServer()
	{
		try 
		{
			if (session != nullptr)
			{
				delete session;
			}
			session = new mysqlx::Session(serverName, portNum, username, password);
			cout << "Connected To Server" << endl;
		}
		catch (std::exception& ex) {}
		catch (const mysqlx::Error& error) {}
	}

	void DataBaseManager::startDataBase()
	{
		connectToServer();
		//session->createSchema("GameMarket", true);
		cout << "DataBase Started. Created/found GameMarket" << endl;
	}
	string readSQLFile(string filePath)
	{
		ifstream file(filePath);
		stringstream buffer;
		buffer << file.rdbuf();
		return buffer.str();
	}
}