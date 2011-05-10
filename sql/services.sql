USE osmarser;

-- Search for duplicate records (hash Tags and Values)
SELECT DISTINCT entity 
FROM dbo.TagsValuesTrans
GROUP BY entity 
HAVING COUNT(*) > 1 