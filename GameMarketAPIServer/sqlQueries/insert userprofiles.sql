use xbox;
start transaction;
insert into userprofiles(xuid, gamertag)  values ("2533274814515397", "RedmptionDenied");
insert into userprofiles(xuid, gamertag)  values ("2533274792073233", "jimmyhova");
insert into userprofiles(xuid, gamertag)  values ("2533274797744336", "SiegfriedX");
insert into userprofiles(xuid, gamertag)  values ("2698138705331816", "Riffai");
insert into userprofiles(xuid, gamertag)  values ("2533274810558996", "True Marvellous");
insert into userprofiles(xuid, gamertag)  values ("2533274880644024", "posiedon11");
insert into userprofiles(xuid, gamertag)  values ("2535419822751112", "x51pegasus50");
commit;

select * from userprofiles;