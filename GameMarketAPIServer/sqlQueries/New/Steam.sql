
select * from steam_appids;
select * from steam_appids where appID = 5709;
select * from steam_appdetails;
select * from steam_appplatforms where appID = 2100;

select * from steam_apppublishers;
select * from steam_appdevelopers;



select count(*) from steam_appids;

select * from steam_appids order by lastScanned;
select * from steam_appids where lastScanned is not null order by lastScanned asc;