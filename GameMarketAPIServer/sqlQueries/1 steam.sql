use steam;

select * from steam.appids order by lastScanned desc;
select count(*) from appids where lastScanned is not null;
select * from appdetails where appname like "Dark";
select * from appdevelopers;
select * from appplatforms;
select * from apppublishers;
select * from packageids;
select * from packages;


select * from appids 
left join appdetails on appdetails.appid = appids.appid
where appids.lastScanned is not null
order by appdetails.lastScanned;

select count(*) from (
select appids.appid from appids 
left join appdetails on appdetails.appid = appids.appid
where appids.lastScanned is not null
order by appdetails.lastScanned, appids.appid) as subquery;

#This combines all the tables
select appdetails.* ,
group_concat(DISTINCT platform order by platform asc SEPARATOR ", ") as platforms,
group_concat(DISTINCT publisher order by publisher asc SEPARATOR ", ") as publishers, 
group_concat(DISTINCT developer order by developer asc SEPARATOR ", ") as developer,
group_concat(DISTINCT packageID order by packageID asc SEPARATOR ", ") as packageIDs
from appdetails
inner join appplatforms on appplatforms.appId = appdetails.appId
inner join apppublishers on apppublishers.appId = appdetails.appId
inner join appdevelopers on appdevelopers.appId = appdetails.appId
inner join packages on packages.appId = appdetails.appId
group by appdetails.appId;


/*Find all titles where its group contails fission and is a 360 game*/
select xbox.gametitles.titlename, xbox.gametitles.moderntitleid, group_concat(xbox.titledevices.device order by xbox.titledevices.device asc SEPARATOR  ', ') as devices, xbox.groupdata.groupName 
from xbox.gametitles
inner join xbox.titledevices on xbox.gametitles.modernTitleID = xbox.titledevices.modernTitleId
inner join xbox.groupdata on xbox.gametitles.groupID = xbox.groupdata.groupid
where xbox.groupdata.groupname like "%[fission]%"
group by xbox.gametitles.moderntitleid
having devices like "%Xbox360%";