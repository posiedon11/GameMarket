use gamemarket;

update xboxgametitles set lastScanned = null where moderntitleId IN (
select modernTitleId from xboxtitledetails group by moderntitleId having count(productID) > 1);

delete from xboxTitleDetails where moderntitleId in (select moderntitleId from xboxtitledetails group by moderntitleid having count(productID) > 1);


DELETE FROM xboxtitledetails
WHERE modernTitleID IN (
    SELECT modernTitleID
    FROM xboxtitledetails
    GROUP BY modernTitleID
    HAVING COUNT(productID) > 1
);

delete from xboxgamebundles where productID in (SELECT productID
    FROM xboxtitledetails
    GROUP BY modernTitleID
    HAVING COUNT(productID) > 1
);
select productID in ( select modernTitleId from xboxtitledetails group by moderntitleid having count(productID) > 1);



CREATE TEMPORARY TABLE temp_productIDs AS
SELECT productID
FROM xboxtitledetails
WHERE modernTitleID IN (
    SELECT modernTitleID
    FROM xboxtitledetails
    GROUP BY modernTitleID
    HAVING COUNT(productID) > 1
);

DELETE FROM xboxgamebundles
WHERE productID IN (SELECT productID FROM temp_productIDs);
DELETE FROM xboxtitledetails
WHERE productID IN (SELECT productID FROM temp_productIDs);
DROP TEMPORARY TABLE IF EXISTS temp_productIDs;


