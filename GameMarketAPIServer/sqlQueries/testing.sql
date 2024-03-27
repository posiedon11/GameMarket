select * from steam.appids;
select count(*) from steam.appids;

select * from xbox.gametitles;
select * from xbox.groupdata;
select * from xbox.titledetails;

select * from xbox.gametitles where titlename like "%V Open Beta%";
select * from xbox.gametitles where groupid like "%f10494ec-9cff-4d88-9d48-728e989587b8%";
select * from xbox.groupdata where groupName like "%oxyna%";
select * from xbox.groupdata where groupName like "%game pass%";

select * from xbox.gametitles where groupID = (select groupId from xbox.groupdata where groupName like "%bailey%");
select count(*) from xbox.groupdata where groupName like "%[fission]%";

select * from xbox.titledevices;
select * from xbox.titledevices where xbox.titledevices.modernTitleId = "1019936697";

select * from xbox.gametitles inner join xbox.titledevices on xbox.gametitles.modernTitleID = xbox.titledevices.modernTitleId where xbox.titledevices.device = "Xbox360";

select * from xbox.gametitles inner join xbox.titledevices on xbox.gametitles.modernTitleID = xbox.titledevices.modernTitleId where xbox.titledevices.device = "PC";

select xbox.gametitles.titlename, xbox.gametitles.moderntitleid, xbox.titledevices.device  from xbox.gametitles inner join xbox.titledevices on xbox.gametitles.modernTitleID = xbox.titledevices.modernTitleId;
select xbox.gametitles.titlename, xbox.gametitles.moderntitleid, xbox.titledevices.device  from xbox.gametitles inner join xbox.titledevices on xbox.gametitles.modernTitleID = xbox.titledevices.modernTitleId where xbox.titledevices.device = "PC";

SELECT xbox.gametitles.titlename, xbox.gametitles.moderntitleid, group_concat(xbox.titledevices.device order by xbox.titledevices.device asc SEPARATOR  ', ') as devices from xbox.gametitles inner join xbox.titledevices on xbox.gametitles.modernTitleID = xbox.titledevices.modernTitleId group by xbox.gametitles.moderntitleid ; 

SELECT xbox.gametitles.titlename, xbox.gametitles.moderntitleid, group_concat(xbox.titledevices.device order by xbox.titledevices.device asc SEPARATOR  ', ') as devices from xbox.gametitles 
inner join xbox.titledevices on xbox.gametitles.modernTitleID = xbox.titledevices.modernTitleId group by xbox.gametitles.moderntitleid ; 


/*Find all titles where its group contails fission and is not a 360 game*/
select xbox.gametitles.titlename, xbox.gametitles.moderntitleid, group_concat(xbox.titledevices.device order by xbox.titledevices.device asc SEPARATOR  ', ') as devices, xbox.groupdata.groupName 
from xbox.gametitles
inner join xbox.titledevices on xbox.gametitles.modernTitleID = xbox.titledevices.modernTitleId
inner join xbox.groupdata on xbox.gametitles.groupID = xbox.groupdata.groupid
where xbox.groupdata.groupname like "%[fission]%"
group by xbox.gametitles.moderntitleid
having not devices like "%Xbox360%";

/*Find all titles where its group contails fission and is a 360 game*/
select xbox.gametitles.titlename, xbox.gametitles.moderntitleid, group_concat(xbox.titledevices.device order by xbox.titledevices.device asc SEPARATOR  ', ') as devices, xbox.groupdata.groupName 
from xbox.gametitles
inner join xbox.titledevices on xbox.gametitles.modernTitleID = xbox.titledevices.modernTitleId
inner join xbox.groupdata on xbox.gametitles.groupID = xbox.groupdata.groupid
where xbox.groupdata.groupname like "%[fission]%"
group by xbox.gametitles.moderntitleid
having devices like "%Xbox360%";


select * from xbox.gametitles where titlename like "%left%";


