﻿## Steps to update Families
0. Open .txt in Excel with comma delim
1. Delete Columns
	1. Vendor_Name
	2. Eagle_SKEW
2. Rename Columns
	1. Vendor_SKEW => Vendor_SKU
	2. Vendor_Notes => Vendor_Notes
3. Create 4 Empty Columns to the left of last column (which should be 'Vendor_Notes')
4. Move Columns and delete the empty column left behind
	1. Manufacturer[-1010108]##OTHER##  -> 1st column
	2. Vendor_SKU  -> 4th column
5. Create columns
	1. EKType##OTHER##   -> 2nd column
	2. EKCategory##OTHER##   -> 3rd column
6. Fill Data for columns
	1. EKType##OTHER##   -> 2nd column
	2. EKCategory##OTHER##   -> 3rd column
7. Confirm the last 5 columns to be exactly same as below  

| Manufacturer[-1010108]##OTHER## | EKType##OTHER## | EKCategory##OTHER## | Vendor_SKU##OTHER## | Vendor_Notes##OTHER## |
| ------------------------------- | --------------- | ------------------- | ------------------- | --------------------- |

## Steps to Family .rfa file
1. Remove Parameter
	1. Vendor_NAME
	1. Vendor_SKEW
2. Rename Parameter
	1. Eagle_SKEW => Vendor_SKU
3. Create 2 new parameters
	- Best way to create the following params is duplicate 'Eagle_SKU'
	1. EKType -> Type Param, Common-Discipline, Length-Data Type, Identity Data-Group Under
	2. EKCategory -> Type Param, Common-Discipline, Length-Data Type, Identity Data-Group Under
4. Reorganize the params in the 'Identity Data' to confirm the following
	1. Manufacturer
	2. EKType
	3. EKCategory
	4. Vendor_SKU_
	5. Vendor_Notes

