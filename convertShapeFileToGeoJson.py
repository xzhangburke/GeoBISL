import geopandas

myshpfile = geopandas.read_file('D:\ETLs\convertShapeFileToGeoJson\in\visn.shp')
myshpfile.to_file('D:\ETLs\convertShapeFileToGeoJson\out\visn.geojson', driver='GeoJSON')