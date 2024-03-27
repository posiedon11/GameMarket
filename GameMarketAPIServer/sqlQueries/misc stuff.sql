use xbox;

#Simple Commands
#Game Title Stuff
select * from xbox.gametitles;
select count(*) from xbox.gametitles;
select count(*) from xbox.gametitles where lastScanned  is not null;

select * from xbox.gametitles where titleName = "Indivisible";
select * from xbox.gametitles where titleName like "%Halo Wars%";
select * from xbox.gametitles where modernTitleID = "2099832516";




#titledetails
select * from xbox.titledetails;
select count(*) from xbox.titledetails;

#product ids
select * from xbox.productids;
select count(*) from xbox.productids;
select * from xbox.productids where productID = "BT5P2X999VH2";

#MarketDedtails
select * from xbox.marketdetails;
##free games
select * from xbox.marketdetails where purchasable is true and msrp =0;

select xbox.marketdetails.* from xbox.marketdetails
	inner join xbox.gamebundles on xbox.gamebundles.relatedProductID = xbox.marketdetails.productID;

#game bundles
select * from xbox.gamebundles;
select * from xbox.gamebundles where productID = "9N25GV4TVG0C";

#group data
select * from xbox.groupData;

#user profiles
select * from xbox.userprofiles;
Select xuid from UserProfiles where lastScanned < "3/16/2024 4:08:11" or lastScanned IS null order by lastScanned;

#devices
select * from xbox.titledevices;
select * from xbox.titledevices where modernTitleId = "2099832516";







#use these for finding groupids with single titles
select xbox.gametitles.titleName from xbox.gametitles join (select groupID from xbox.gametitles group by groupID having count(*) = 1) as singleGroups on xbox.gametitles.groupid = singleGroups.groupID;
select count(xbox.gametitles.titleName) from xbox.gametitles join (select groupID from xbox.gametitles group by groupID having count(*) = 1) as singleGroups on xbox.gametitles.groupid = singleGroups.groupID;

#use these for finding groupids with single titles
select xbox.gametitles.titleName from xbox.gametitles join (select groupID from xbox.gametitles group by groupID having count(*) > 1) as multiGroups on xbox.gametitles.groupid = multiGroups.groupID;
select count(xbox.gametitles.titleName) from xbox.gametitles join (select groupID from xbox.gametitles group by groupID having count(*) > 1) as multiGroups on xbox.gametitles.groupid = multiGroups.groupID;


#Use this to find all mobile games
select gametitles.*, group_concat(xbox.titledevices.device order by xbox.titledevices.device asc SEPARATOR  ', ') as devices 
from xbox.gametitles 
join xbox.titledetails on xbox.gametitles.modernTitleID = xbox.titledetails.modernTitleID
inner join xbox.titledevices on gametitles.modernTitleID = titledevices.modernTitleID
#where gametitles.modernTitleID = "2099832516" #like "%Time Traveller%" 
group by moderntitleid
having devices like "%Mobile%";



select * from xbox.groupdata where groupid = (select groupID from xbox.gametitles group by groupid having count(groupID) > 1);
