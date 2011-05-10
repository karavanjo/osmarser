--select au_fname, au_lname
--    from authors a
--        where exists (
--                                 select *
--                                 from titleAuthor t
--                                 where t.au_id = a.au_id
--                                )
SELECT DISTINCT tagsValues.tag, tagsValuesTrans.val
FROM dbo.TagsValues AS tagsValues
JOIN dbo.TagsValuesTrans AS tagsValuesTrans
ON (tagsValues.tag=tagsValuesTrans.entity)
WHERE tagsValuesTrans.codeLang=-1
AND tagsValuesTrans.val='highway'

SELECT DISTINCT tagsValues.vHash, tagsValuesTrans.val
FROM dbo.TagsValues AS tagsValues
JOIN dbo.TagsValuesTrans AS tagsValuesTrans
ON (tagsValues.vHash=tagsValuesTrans.entity)
WHERE tagsValuesTrans.codeLang=-1
AND tagsValues.tag=953493373
