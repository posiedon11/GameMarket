select * from xbox_gametitle;
select * from xbox_marketdetail;


select * from xbox_gametitle where moderntitleid = 947714459;



select * from xbox_gametitle where lastScanned is not null order by lastScanned desc;
select * from xbox_productid where lastScanned is not null order by lastScanned desc;





select * from xbox_userprofile;
select count(*) from xbox_gametitle;
select count(*) from xbox_marketdetail;