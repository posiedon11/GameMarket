use gamemarket;

create table xboxgamebundles(
relatedProductID char(15) not null,
productID char(15) not null,
lastScanned datetime default null,
foreign key (productID) references xboxtitledetails(productID),
primary key (relatedProductID)
);
