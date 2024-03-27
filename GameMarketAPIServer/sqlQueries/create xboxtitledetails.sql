use xbox;

drop table if exists titledetails;
create table titledetails(
modernTitleID char (15) not null,
productID char (15) not null,
foreign key (modernTitleID) references gametitles(modernTitleID) on delete cascade,
foreign key (productID) references productids(productID) on delete cascade,
primary key (productID)
);