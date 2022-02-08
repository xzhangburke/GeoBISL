import arcpy
import os
import time

#inputTable = arcpy.GetParameter(0)
#When running in the python window in ArcMap, I comment out line 3 above and un-comment line 6.  

currentTime = str(round(time.time() * 1000))

inputTable = r"D:\ETLs\AggregatePoints\data_in\test.csv"

polygon_workspace = r"D:\ETLs\AggregatePoints\data_polygon\VA_Data.gdb"
polygon_features = os.path.join(polygon_workspace, "Markets")

out_workspace = r"D:\ETLs\AggregatePoints\data_out"
out_csv = os.path.join(out_workspace, "out_" + currentTime + ".xls")


scratch_workspace = r"D:\ETLs\AggregatePoints\scratch\scratch.gdb"

arcpy.env.workspace = scratch_workspace
cws = arcpy.env.workspace

fc_name = "tableXYfc_" + currentTime
tableXY = "tableXY_" + currentTime

scratch_fc = os.path.join(cws, fc_name)

joinFc = "in_memory\\_join_" + currentTime

print(fc_name, tableXY, joinFc)

if arcpy.Exists(tableXY):
    arcpy.Delete_management(tableXY)
if arcpy.Exists(scratch_fc):
    arcpy.Delete_management(scratch_fc)
if arcpy.Exists(joinFc):
    arcpy.Delete_management(joinFc)


arcpy.MakeXYEventLayer_management(inputTable, "x", "y", tableXY)
    
arcpy.FeatureClassToFeatureClass_conversion(tableXY, cws, fc_name)

arcpy.SpatialJoin_analysis(scratch_fc, polygon_features, joinFc,"","")

arcpy.TableToExcel_conversion(joinFc, out_csv)


if arcpy.Exists(tableXY):
    arcpy.Delete_management(tableXY)
if arcpy.Exists(scratch_fc):
    arcpy.Delete_management(scratch_fc)
if arcpy.Exists(joinFc):
    arcpy.Delete_management(joinFc)




# Set local variables
point_workspace = r"D:\ETLs\AggregatePoints\data_in\VA_Points.gdb"

outWorkspace = r"D:\ETLs\AggregatePoints\data_in\output.gdb"
 
# Want to join USA cities to states and calculate the mean city population
# for each state
point_features = os.path.join(point_workspace, "testing_data")
polygon_features = os.path.join(polygon_workspace, "Markets")
 
# Output will be the target features, states, with a mean city population field (mcp)
outfc = os.path.join(outWorkspace, "aggregatePoints")
 
# Create a new fieldmappings and add the two input feature classes.
#fieldmappings = arcpy.FieldMappings()
#fieldmappings.addTable(polygon_features)
#fieldmappings.addTable(point_features)
 
# First get the POP1990 fieldmap. POP1990 is a field in the cities feature class.
# The output will have the states with the attributes of the cities. Setting the
# field's merge rule to mean will aggregate the values for all of the cities for
# each state into an average value. The field is also renamed to be more appropriate
# for the output.
#pop1990FieldIndex = fieldmappings.findFieldMapIndex("POP1990")
#fieldmap = fieldmappings.getFieldMap(pop1990FieldIndex)
 
# Get the output field's properties as a field object
#field = fieldmap.outputField
 
# Rename the field and pass the updated field object back into the field map
#field.name = "mean_city_pop"
#field.aliasName = "mean_city_pop"
#fieldmap.outputField = field
 
# Set the merge rule to mean and then replace the old fieldmap in the mappings object
# with the updated one
#fieldmap.mergeRule = "mean"
#fieldmappings.replaceFieldMap(pop1990FieldIndex, fieldmap)
 
# Delete fields that are no longer applicable, such as city CITY_NAME and CITY_FIPS
# as only the first value will be used by default
#x = fieldmappings.findFieldMapIndex("CITY_NAME")
#fieldmappings.removeFieldMap(x)
#y = fieldmappings.findFieldMapIndex("CITY_FIPS")
#fieldmappings.removeFieldMap(y)
 
#Run the Spatial Join tool, using the defaults for the join operation and join type
#arcpy.SpatialJoin_analysis(targetFeatures, joinFeatures, outfc, "#", "#", fieldmappings)

