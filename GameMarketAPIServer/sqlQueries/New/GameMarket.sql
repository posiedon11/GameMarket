select * from gamemarket_gametitle;
select * from gamemarket_gametitle where gameTitle like "%borderlands%";

select * from gamemarket_developer;
select * from gamemarket_publisher;

select * from gamemarket_xboxlink;
select * from gamemarket_steamlink;


select gamemarket_gametitle.*, appID, modernTitleID from gamemarket_gametitle
inner join gamemarket_xboxlink on gamemarket_xboxlink.gameID = gamemarket_gametitle.gameID
inner join gamemarket_steamlink on gamemarket_steamlink.gameID = gamemarket_gametitle.gameID;

select gamemarket_gametitle.*, appID, modernTitleID from gamemarket_gametitle
left join gamemarket_xboxlink on gamemarket_xboxlink.gameID = gamemarket_gametitle.gameID
left join gamemarket_steamlink on gamemarket_steamlink.gameID = gamemarket_gametitle.gameID;

select gamemarket_gametitle.*, appID, modernTitleID from gamemarket_gametitle
left join gamemarket_xboxlink on gamemarket_xboxlink.gameID = gamemarket_gametitle.gameID
left join gamemarket_steamlink on gamemarket_steamlink.gameID = gamemarket_gametitle.gameID
where gameTitle like "%borderlands%";


select count(*) from gamemarket_gametitle;