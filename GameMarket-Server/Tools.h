#pragma once
#ifndef TOOLS_LOCK
#define TOOLS_LOCK

#include<string>
#include<sstream>
#include<fstream>
#include<iostream>
#include<rapidjson/rapidjson.h>
#include<rapidjson/document.h>
#include<vector>
#include<chrono>
#include<ctime>
#include<iomanip>
#include<map>
#include<atomic>
#include<thread>
#include<functional>

using namespace std;
namespace tools_namespace
{
    vector<std::string> readFromFile(const std::string& filePath);
    size_t writeCallBack(char* contents, size_t size, size_t nmemb, void* userdata);
    size_t headerCallBack(char* contents, size_t size, size_t nmemb, void* userdata);

    bool inDateRange(const std::string &start, const std::string &end);
    

    //use if there are no arrays in path
    const rapidjson::Value* nestedFind(const std::string& path, const rapidjson::Value& root);

    //use if you want to get all values in arrays
    vector<const rapidjson::Value*> recursiveFind(const std::string& path, const rapidjson::Value& root, bool returnArray = false);
}
#endif // !TOOLS_LOCK
