use gamemarket;

drop table xboxtitledetails;
create table xboxtitledetails(
titleID char (15) not null,
productID char (15) not null,
lastScanned datetime default null,
foreign key (titleID) references xboxgametitles(titleID),
primary key (productID)
);
