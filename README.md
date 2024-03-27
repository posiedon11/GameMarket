# GameMarket
# Installation
Download the repository, and install MYSQL community server (Im working on windows so there may be differences)
https://dev.mysql.com/downloads/mysql/
Im using visual studio 2022.
Create a MYSQL connection, and create three schemas, "steam" "gamemarket" and "xbox". make sure that lower_case_table_names is disabled.

Open GameMarket/GameMarketAPIServer and open the sln file.
if the nugget files dont download/restore.
#Nugget group
<ItemGroup>
  <PackageReference Include="FuzzySharp" Version="2.0.2" />
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.3" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
  <PackageReference Include="Moq" Version="4.20.70" />
  <PackageReference Include="MySqlConnector" Version="2.3.5" />
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  <PackageReference Include="SteamKit2" Version="2.5.0" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
  <PackageReference Include="xunit" Version="2.7.0" />
  <PackageReference Include="xunit.runner.console" Version="2.7.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>

