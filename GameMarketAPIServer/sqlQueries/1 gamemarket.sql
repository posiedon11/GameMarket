use gamemarket;

select * from gametitles;
select * from xboxlink;


select gametitles.*,
group_concat(Distinct xboxlink.modernTitleID order by xboxlink.modernTitleID asc SEPARATOR  ', ') as modernIds 
 from gamemarket.gametitles
inner join gamemarket.xboxlink on gametitles.gameId = xboxlink.gameID
group by gameid;

select gametitles.*,
group_concat(Distinct xboxlink.modernTitleID order by xboxlink.modernTitleID asc SEPARATOR  ', ') as modernIds 
 from gamemarket.gametitles
inner join gamemarket.xboxlink on gametitles.gameId = xboxlink.gameID
where gametitle like "%modern warfare%"
group by gameid;