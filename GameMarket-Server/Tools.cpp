#include"Tools.h"
namespace tools_namespace
{
    vector<std::string> readFromFile(const std::string& filePath) {
        std::ifstream file(filePath);
        std::stringstream buffer;
        std::vector<std::string> results;
        if (file.fail())
        {
            std::cout << "File failed to open"<< std::endl;
        }
        buffer << file.rdbuf();
       
        std::string st = buffer.str();
        if (st == "")
            std::cout << "File is empty" << std::endl;
        else
        {
            std::istringstream inputStream(st);
            std::string line;
            while (std::getline(inputStream, line, ';'))
            {
                line.erase(0, line.find_first_not_of(" \n\r\t"));
                line.erase(line.find_last_not_of(" \n\r\t") + 1);

                if (!line.empty())
                {
                    results.push_back(line);
                }
            }
            std::cout << "Found File" << std::endl;
        }
        return results;
    }

    bool inDateRange(const std::string &start, const std::string &end)
    {

        auto now = std::chrono::system_clock::now();

        std::tm start_tm{}, end_tm{};
        std::istringstream start_ss{ start };
        std::istringstream end_ss{ end };
        start_ss >> std::get_time(&start_tm, "%Y-%m-%dT%H:%M:%S");
        end_ss >> std::get_time(&end_tm, "%Y-%m-%dT%H:%M:%S");

        auto start_time = std::chrono::system_clock::from_time_t(std::mktime(&start_tm));
        auto end_time = std::chrono::system_clock::from_time_t(std::mktime(&end_tm));
        
        
        return now >= start_time && now <= end_time;
    }

    //path is format "a/b/c..."
    const rapidjson::Value* nestedFind(const std::string& path, const rapidjson::Value& root)
    {
        //spilit input into vector;
        std::vector<std::string> parts;
        std::string p;
        std::istringstream partsStream(path);
        while (std::getline(partsStream, p, '/'))
        {
            parts.push_back(p);
        }
        const rapidjson::Value* current = &root;


        for (const auto& part : parts)
        {
            if (current->IsObject() && current->HasMember(part.c_str()))
            {
                current = &(*current)[part.c_str()];
                
                if (part == parts.back())
                {
                    return current;
                }
            }
            else if (current->IsArray())
            {
                return nullptr;
            }
            else
            {
                return nullptr;
            }
        }
    }


    vector<const rapidjson::Value*> recursiveFind(const std::string& path, const rapidjson::Value& root, bool returnArray)
    {
        std::vector<std::string> parts;
        std::vector<const rapidjson::Value*> values;
        std::string p;
        std::istringstream partsStream(path);
        while (std::getline(partsStream, p, '/'))
        {
            parts.push_back(p);
        }


        std::function<void(const rapidjson::Value&, size_t)> traverse = [&](const rapidjson::Value& current, size_t index)
            {
                if (index >= parts.size()) {
                    if (current.IsArray())
                    {
                        for (const auto& item : current.GetArray())
                            values.push_back(&item);
                    }
                    else
                     values.push_back(&current);
                    return; // End of path
                }

                const string& part = parts[index];

                if (current.IsObject())
                {
                    const auto &itr = current.FindMember(part.c_str());
                    if (itr != current.MemberEnd())
                    {
                        traverse(itr -> value, ++index);
                    }
                }
                else if (current.IsArray() )
                {
                    if (index == parts.size() - 1)
                    {

                        for (const auto& item : current.GetArray())
                        {
                            const auto& child = item.FindMember(part.c_str());
                            if (child != item.MemberEnd())
                            {
                                if (returnArray)
                                    values.push_back(&child->value);
                                else
                                    traverse(child->value, ++index);
                            }
                        }
                        return;
                    }
                    else
                    {
                        for (const auto& item : current.GetArray())
                        {
                            const auto& child = item.FindMember(part.c_str());
                            if (child != item.MemberEnd())
                                traverse(child->value, 0);
                        }
                    }
                }
            };
        traverse(root, 0);
        return values;
    }


     size_t writeCallBack(char* contents, size_t size, size_t nmemb, void* userdata) {

        static_cast<std::string*>(userdata)->append(contents, size * nmemb);
        return size * nmemb;
    }
     size_t headerCallBack(char* contents, size_t size, size_t nmemb, void* userdata)
    {
        string header(contents, size * nmemb);
        auto* headers = static_cast<std::map<std::string, std::string>*>(userdata);
        auto pos = header.find(':');
        if (pos != std::string::npos)
        {
            string name = header.substr(0, pos);
            string value = header.substr(pos + 2, header.length() - pos - 3);

            (*headers)[name] = value;
        }
        return size * nmemb;
    }
}