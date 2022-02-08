
var webmapId = "35027e6736df485fbe6c914e7ec2e9a9"
var webserviceUrl = "https://gis.va.gov/NCA_WebService/NCAService.asmx";

var scheduleUrl = "https://dailyburialschedule.cem.va.gov/CBS/";
var databookUrl = "http://vaww.nca.va.gov/databook";
var portalUrl = "https://gis.va.gov/portal/sharing/content/items";
var photoUrl = "https://gis.va.gov/nca_photos/";
var cemeteryServiceName = "Cemetery Boundary";
var layer_graveSitePoint_name = "Gravesite Marker";
var layer_intermentPoint_name = "Interment Point";
var layer_gravesitePolygon_name = "Gravesite";
var layer_section_name = "Section";
var layer_sitePoint_name = "Site";
var layer_sitePolygon_name = "Site Boundary";
var layer_district_name = "District Boundary";

var fields_notUse = ["OBJECTID", "ACTIVE", "POSITION_SRC", "CREATED_USER", "CREATED_DATE", "LAST_EDITED_USER", "LAST_EDITED_DATE", "SHAPE", "SHAPE.STAREA()", "SHAPE.STLENGTH", "SORT_BY", "FRONT_IMG_URL", "REAR_IMG_URL", "GLOBALID"];
var gravesiteScale = 1000;

var map;
var navToolbar;
var loading;

// bookmarker
var storageName = 'esrijsapi_mapmarks';
var useLocalStorage;
var bookmark;
var bmJSON;
			
var identifyTask;
var identifyParams;
var identifyTask_site;
var identifyParams_site;
var identifyTask_gravesite_polygon;
var identifyParams_gravesite_polygon;

var url_service = "";
var url_district = "";
var url_site = "";
var url_site_polygon = "";
var url_section_polygon = "";

var layerList;
var initial_map_extent;
var layerList
var url_gravemarker = "";
var gLayer;
var infoSymbol;
var photoList = [];
var sectionExtent = "";


var mapImageLayer_cemetery = "";
var mapImageLayer_cemetery_url = "";
var layer_graveSitePoint_index = -1;
var layer_intermentPoint_index = -1;
var layer_gravesitePolygon_index = -1;
var layer_section_index = -1;
var layer_sitePoint_index = -1;
var layer_sitePolygon_index = -1;
var layer_district_index = -1;

var layer_graveSitePoint;
var layer_intermentPoint;
var layer_gravesitePolygon;
var layer_sectionPolygon;
var layer_sitePoint;
var layer_sitePolygon;
var layer_districtPolygon;

var currentUser = "";
var editorStatus = false;
var firstName_array = [];
var lastName_array = [];
var userInfo_array = [];
var allUser_status = true;
var gravesiteList_array = [];
var domainList_gravesite_marker = [];
var domainList_site_point = [];

var site_id_current = "";
var section_current = "";
var gravesite_id_current = "";
var siteList_current = [];


require([
  "esri/map",
  "esri/toolbars/navigation",
  "dojo/on",  
  "esri/geometry/Polygon",
  "esri/geometry/Point",
  "esri/SpatialReference",
  "esri/tasks/IdentifyTask",
  "esri/tasks/IdentifyParameters",
  "esri/layers/GraphicsLayer",
  "esri/dijit/HomeButton",
  "esri/dijit/Search",
  "esri/dijit/Bookmarks",
  "esri/dijit/Scalebar",
  "esri/geometry/scaleUtils",
  "esri/tasks/QueryTask",
  "esri/tasks/query",
  "esri/dijit/BasemapGallery",
  "esri/arcgis/utils",
  "esri/dijit/LayerList",
  "esri/dijit/Legend",
  "esri/dijit/Measurement",
  "dojo/parser",
  "esri/SpatialReference",
  "esri/tasks/GeometryService",
  "esri/config",
  "esri/graphic",
  "esri/symbols/SimpleMarkerSymbol",
  "esri/symbols/SimpleFillSymbol",
  "esri/Color",
  "dojo/dom",
  "dojo/dom-attr",
  "dijit/form/DropDownButton",
  "dijit/DropDownMenu",
  "dijit/Menu",
  "dijit/MenuItem",
  "dijit/layout/BorderContainer",
  "dijit/layout/ContentPane",
  "dijit/TitlePane",
  "dijit/Dialog",
  "dijit/registry",
  "dijit/form/Button",
  "dijit/Toolbar",
  "dijit/ToolbarSeparator",
  "dojo/dom-construct",
  "dojo/domReady!"
], function(
  Map, Navigation, on, Polygon, Point, SpatialReferenc, IdentifyTask, IdentifyParameters, GraphicsLayer, Search, HomeButton, Bookmarks, Scalebar, scaleUtils, QueryTask, Query, BasemapGallery, arcgisUtils, LayerList, Legend, Measurement,
  parser, SpatialReference, GeometryService, esriConfig, Graphic, SimpleMarkerSymbol, SimpleFillSymbol, Color, dom, domAttr, DropDownButton, Menu, DropDownMenu, MenuItem, Dialog, registry, domConstruct, ready
) {
	parser.parse();
	//esriConfig.defaults.io.alwaysUseProxy = false;

	// init map staff
	function init()
	{
		//
		arcgisUtils.arcgisUrl = portalUrl;
		arcgisUtils.createMap(webmapId, "map").then(function (response) {
			map = response.map;
			
			//
			navToolbar = new Navigation(map);
			on(navToolbar, "onExtentHistoryChange", extentHistoryChangeHandler);
		  
			// Search
			var search = new Search({
				map: map
			 }, "search");
			 search.startup();
			 
			// HomeButton
			var home = new HomeButton({
				map: map
			}, "HomeButton");
			home.startup();
		 
			// BookMark
			useLocalStorage = supports_local_storage();
			bookmark = new esri.dijit.Bookmarks({
				map: map, 
				bookmarks: [],
				editable: true
			}, dojo.byId('bookmarkDiv'));

			dojo.connect(bookmark, 'onEdit', refreshBookmarks);
			dojo.connect(bookmark, 'onRemove', refreshBookmarks);

			// Look for stored bookmarks
			if ( useLocalStorage ) {
				bmJSON = window.localStorage.getItem(storageName);
			} else {
				bmJSON = dojo.cookie(storageName);
			}
			if ( bmJSON && bmJSON != 'null' && bmJSON.length > 4) {
				var bmarks = dojo.fromJson(bmJSON);
				dojo.forEach(bmarks, function(b) {
					//console.log(b)
					//bookmark.addBookmark(b);
				});
			}

			
			// basemapGallery
			var basemapGallery = new BasemapGallery({
				showArcGISBasemaps: true,
				map: map
			}, "basemapGalleryDiv");
			basemapGallery.startup();
			basemapGallery.on("error", function(msg) {
				console.log("basemap gallery error:  ", msg);
			});		
			
			// LayerList
			layers = arcgisUtils.getLayerList(response);
			layerList = layers;
			var layersWidget = new LayerList({
				map: response.map,
				layers: layers
			},"layerListDiv");
			layersWidget.startup();

			// Legend
			var legendLayers = arcgisUtils.getLegendLayers(response);
			var legendDijit = new Legend({
				map: map,
				layerInfos: legendLayers
			}, "legendDiv");
			legendDijit.startup();

			// measurement
			var measurement = new Measurement({
				map: map
			}, dom.byId("measurementDiv"));
			measurement.startup();
		
			// scalebar
			var scalebar = new Scalebar({
			  map: map,
			  scalebarUnit: "dual"
			});
			
			gLayer = new GraphicsLayer();
			map.addLayer(gLayer);
			
			infoSymbol = new esri.symbol.PictureMarkerSymbol({
				"angle": 0,
				"xoffset": 0,
				"yoffset": 12,
				"type": "esriPMS",
				"url": "img/YellowStickpin.png",
				"contentType": "image/png",
				"width": 30,
				"height": 30
			});
			
			// get list of items
			var layerInfos;
			var layer;
			dojo.forEach(layerList, function (item) {
				layerInfos = item.layer.layerInfos;
				dojo.forEach(layerInfos, function (item) {
					//console.log(item.id, item.name)
					if (item.name == layer_graveSitePoint_name) layer_graveSitePoint_index = item.id;
					if (item.name == layer_intermentPoint_name) layer_intermentPoint_index = item.id;
					if (item.name == layer_gravesitePolygon_name) layer_gravesitePolygon_index = item.id;
					if (item.name == layer_section_name) layer_section_index = item.id;
					if (item.name == layer_sitePoint_name) layer_sitePoint_index = item.id;
					if (item.name == layer_sitePolygon_name) layer_sitePolygon_index = item.id;
					if (item.name == layer_district_name) layer_district_index = item.id;
				});
			});

			for(var j = 0; j < map.layerIds.length; j++) {
				var layer_tmp = map.getLayer(map.layerIds[j]);
				if (layer_tmp.arcgisProps["title"].indexOf(cemeteryServiceName) >= 0){
					mapImageLayer_cemetery = layer_tmp;
					mapImageLayer_cemetery_url = mapImageLayer_cemetery['url'];
				}
			}

			//console.log("mapImageLayer_cemetery", mapImageLayer_cemetery);
			//console.log("mapImageLayer_cemetery_url", mapImageLayer_cemetery_url);
			//console.log("layer_graveSitePoint_index", layer_graveSitePoint_index);
			//console.log("layer_intermentPoint_index", layer_intermentPoint_index);
			//console.log("layer_gravesitePolygon_index", layer_gravesitePolygon_index);
			//console.log("layer_section_index", layer_section_index);
			//console.log("layer_sitePoint_index", layer_sitePoint_index);
			//console.log("layer_sitePolygon_index", layer_sitePolygon_index);
			//console.log("layer_district_index", layer_district_index);

			var layerStatus = true;
			
			if (mapImageLayer_cemetery == "") layerStatus = false;
			if (layer_graveSitePoint_index == -1) layerStatus = false;
			if (layer_intermentPoint_index == -1) layerStatus = false;
			if (layer_gravesitePolygon_index == -1) layerStatus = false;
			if (layer_section_index == -1) layerStatus = false;
			if (layer_sitePoint_index == -1) layerStatus = false;
			if (layer_sitePolygon_index == -1) layerStatus = false;
			if (layer_district_index == -1) layerStatus = false;
			
			if (!layerStatus)
			{
				sendAlert("<font color='red'>Error: loading map layers.Contact system admin.</font>");
			}
			
			layer_graveSitePoint = layerInfos[layer_graveSitePoint_index];
			layer_intermentPoint = layerInfos[layer_intermentPoint_index];
			layer_gravesitePolygon = layerInfos[layer_gravesitePolygon_index];
			layer_sectionPolygon = layerInfos[layer_section_index];
			layer_sitePoint = layerInfos[layer_sitePoint_index];
			layer_sitePolygon = layerInfos[layer_sitePolygon_index];
			layer_districtPolygon = layerInfos[layer_district_index];

			url_service = mapImageLayer_cemetery_url;
			url_district = url_service +"/" + layer_district_index + "/query?where=1%3D1&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=DISTRICT&returnGeometry=false&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&queryByDistance=&returnExtentsOnly=false&datumTransformation=&parameterValues=&rangeValues=&f=pjson";
			url_site = url_service +"/" + layer_sitePoint_index + "/query?where=1%3D1&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=SITE_ID%2CSHORT_NAME&returnGeometry=false&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&queryByDistance=&returnExtentsOnly=false&datumTransformation=&parameterValues=&rangeValues=&f=pjson";
			url_site_polygon = url_service +"/" + layer_sitePolygon_index;
			url_section_polygon = url_service +"/" + layer_section_index;
			url_gravemarker = url_service +"/" + layer_graveSitePoint_index;

			// set default definitionExpress
			var queryStr = "SITE_ID = '-9999'";
			mapImageLayer_cemetery.layerDefinitions[layer_graveSitePoint_index] = queryStr;
			mapImageLayer_cemetery.layerDefinitions[layer_intermentPoint_index] = queryStr;
			mapImageLayer_cemetery.layerDefinitions[layer_gravesitePolygon_index] = queryStr;


			// identify
			map.on("click", identifyGravesiteMarker);
			
			// gravesite identify
			identifyTask = new IdentifyTask(url_service);
			identifyParams = new IdentifyParameters();
			identifyParams.tolerance = 3;
			identifyParams.returnGeometry = true;
			identifyParams.layerIds = [layer_graveSitePoint_index];
			identifyParams.layerOption = IdentifyParameters.LAYER_OPTION_ALL;
			identifyParams.width = map.width;
			identifyParams.height = map.height;

			// gravesite_polygon identify
			identifyTask_gravesite_polygon = new IdentifyTask(url_service);
			identifyParams_gravesite_polygon = new IdentifyParameters();
			identifyParams_gravesite_polygon.tolerance = 3;
			identifyParams_gravesite_polygon.returnGeometry = true;
			identifyParams_gravesite_polygon.layerIds = [layer_gravesitePolygon_index];
			identifyParams_gravesite_polygon.layerOption = IdentifyParameters.LAYER_OPTION_ALL;
			identifyParams_gravesite_polygon.width = map.width;
			identifyParams_gravesite_polygon.height = map.height;

			// site identify
			identifyTask_site = new IdentifyTask(url_service);
			identifyParams_site = new IdentifyParameters();
			identifyParams_site.tolerance = 10;
			identifyParams_site.returnGeometry = true;
			identifyParams_site.layerIds = [layer_sitePoint_index];
			identifyParams_site.layerOption = IdentifyParameters.LAYER_OPTION_ALL;
			identifyParams_site.width = map.width;
			identifyParams_site.height = map.height;
			
			map.setMapCursor("default");
			
			// populated data
			getUserName();
			populateSiteList();
			getDomainList();
			
			//
			var btn_previous = dijit.byId('btn_previous').domNode;
			var btn_next = dijit.byId('btn_next').domNode;
			dojo.style(btn_previous, {display:'block'});
			dojo.style(btn_next, {display:'block'});

			// dojo button click event
			dijit.registry.byId("btn_peopleSubmit").on("click", function() {
				btn_peopleSubmit();
			
			});
			//
			dijit.registry.byId("btn_summaryReport").on("click", function() {
				btn_summaryReport();
			
			});
			//
			dijit.registry.byId("scheduler_btn").on("click", function() {
				$("#schedule_iframe").attr("src", scheduleUrl);
				schedulerDialog.show();
			});
			// 
			dijit.registry.byId("editor_btn").on("click", function() {
				document.getElementById("select_site_editor").selectedIndex = -1;
				document.getElementById("select_section_editor").selectedIndex = -1;
				document.getElementById("select_gravesite_editor").selectedIndex = -1;
				document.getElementById("select_gravesiteStatus").selectedIndex = -1;
				$('#input_gravesite_poly_objectid').val("");
				//
				editorDialog.show();
			});
			
			//
			dijit.registry.byId("btn_gravesiteReport").on("click", function() {
				btn_gravesiteReport();
			});
			//
			dijit.registry.byId("btn_mapbookReport").on("click", function() {
				btn_mapbookReport();
			});
			//
			dijit.registry.byId("btn_editor_submit").on("click", function() {
				btn_editor_submit();
			});

			//
			dijit.registry.byId("btn_previous").on("click", function () {
				navToolbar.zoomToPrevExtent();
			});
			dijit.registry.byId("btn_next").on("click", function () {
				navToolbar.zoomToNextExtent();
			});

			function extentHistoryChangeHandler () {
				registry.byId("btn_previous").disabled = navToolbar.isFirstExtent();
				registry.byId("btn_next").disabled = navToolbar.isLastExtent();
			}
		});
	}

		  
	// control actions
	$('#select_site_people').change(function() {
		$('#select_gravesite_people').empty();

		var site_id = this.value.split(' (')[1].replace(')', '');
		//
		$('#firstname_field').val('');
		$('#lastname_field').val('');

		getIntermentNameList(site_id);
		populateAutoComplete();
	});
	
	$('#select_site_report').change(function() {
		var site_id = this.value.split(' (')[1].replace(')', '');

		// check user permission
		if (!allUser_status){
			var status = false;
			if (userinfo_array.length > 0){
				for (var i = 0; i < userinfo_array.length; i++)
				{
					var userinfo = userinfo_array[i];
					var site_current = userinfo["site_id"];
					if (site_current == site_id)
					{
						status = true;
						break;
					}
				}
			}
			if (!status){
				var message = "You don't have a permission to this site data(site_id = " + site_id + ").\nPlease contact the site administrator.";
				sendAlert(message);
				return;
			}
		}

		//
		var select_section_report = document.getElementById('select_section_report');
		select_section_report.options.length = 0;
		
		var select_gravesite_report = document.getElementById('select_gravesite_report');
		select_gravesite_report.options.length = 0;

		var opt = document.createElement("option");
		opt.value= "Select SECTION";
		opt.innerHTML = "Select SECTION";
		select_section_report.appendChild(opt);

		
		var url_webservice = webserviceUrl + "/getSectionListFromSite";
		var parameters = "{'site_id':'" + site_id + "'}";
		
		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				var status = "";
				var jsonData_section = "";
				
				$.each(jsonData, function(item, value){
					if (item == "status"){
						status = value;
					}
					if (item == "sectionSiteData"){
						jsonData_section = value;
					}
				});
				if (status == "OK"){
					$.each(jsonData_section, function(item, value){
						var section = value["section"];
						var opt = document.createElement("option");
						opt.value= section;
						opt.innerHTML = section;
						select_section_report.appendChild(opt);
					});
				}
			},
			error: function(e){
				console.log("error");
			}
		});			
	});

	$('#select_site_editor').change(function() {
		$('#input_gravesite_poly_objectid').val("");
		$('#select_gravesiteStatus').val("");
		$("label[for='btn_editor_submit']").html("");

		var select_section_editor = document.getElementById('select_section_editor');
		select_section_editor.options.length = 0;

		var select_gravesite_editor = document.getElementById('select_gravesite_editor');
		select_gravesite_editor.options.length = 0;

		document.getElementById("select_gravesiteStatus").selectedIndex = -1;
		$("#select_gravesiteStatus").prop("disabled", true);
		dijit.byId("btn_editor_submit").setAttribute('disabled', true);
		$("label[for='btn_editor_submit']").html("");		

		var site_id = this.value.split(' (')[1].replace(')', '');

		// check user permission
		if (!allUser_status){
			var status = false;
			if (userinfo_array.length > 0){
				for (var i = 0; i < userinfo_array.length; i++)
				{
					var userinfo = userinfo_array[i];
					var site_current = userinfo["site_id"];
					if (site_current == site_id)
					{
						status = true;
						break;
					}
				}
			}
			if (!status){
				var message = "You don't have a permission to this site data(site_id = " + site_id + ").\nPlease contact the site administrator.";
				sendAlert(message);
				return;
			}
		}

		//
		
		var opt = document.createElement("option");
		opt.value= "Select SECTION";
		opt.innerHTML = "Select SECTION";
		select_section_editor.appendChild(opt);

		
		var url_webservice = webserviceUrl + "/getSectionListFromSite";
		var parameters = "{'site_id':'" + site_id + "'}";
		
		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				var status = "";
				var jsonData_section = "";
				
				$.each(jsonData, function(item, value){
					if (item == "status"){
						status = value;
					}
					if (item == "sectionSiteData"){
						jsonData_section = value;
					}
				});
				if (status == "OK"){
					$.each(jsonData_section, function(item, value){
						var section = value["section"];
						var opt = document.createElement("option");
						opt.value= section;
						opt.innerHTML = section;
						select_section_editor.appendChild(opt);
					});
					// select section 
					if (site_id_current != ""){
						var s = document.getElementById("select_section_editor");
						for ( var i = 0; i < s.options.length; i++ ) {
							if ( s.options[i].value == section_current) {
								$("#select_section_editor").val(s.options[i].value).trigger("change");
								break;
							}
						}
					}
					
				}
			},
			error: function(e){
				console.log("error");
			}
		});			
	});
		
	$('#select_site_mapbook').change(function() {
		var site_id = this.value.split(' (')[1].replace(')', '');

		// check user permission
		if (!allUser_status){
			var status = false;
			if (userinfo_array.length > 0){
				for (var i = 0; i < userinfo_array.length; i++)
				{
					var userinfo = userinfo_array[i];
					var site_current = userinfo["site_id"];
					if (site_current == site_id)
					{
						status = true;
						break;
					}
				}
			}
			if (!status){
				var message = "You don't have a permission to this site data(site_id = " + site_id + ").\nPlease contact the site administrator.";
				sendAlert(message);
				return;
			}
		}

		//
		var select_section_mapbook = document.getElementById('select_section_mapbook');
		select_section_mapbook.options.length = 0;
		
		var opt = document.createElement("option");
		opt.value= "Select SECTION";
		opt.innerHTML = "Select SECTION";
		select_section_mapbook.appendChild(opt);

		
		var url_webservice = webserviceUrl + "/getSectionListFromSite";
		var parameters = "{'site_id':'" + site_id + "'}";
		
		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				var status = "";
				var jsonData_section = "";
				
				$.each(jsonData, function(item, value){
					if (item == "status"){
						status = value;
					}
					if (item == "sectionSiteData"){
						jsonData_section = value;
					}
				});
				if (status == "OK"){
					$.each(jsonData_section, function(item, value){
						var section = value["section"];
						var opt = document.createElement("option");
						opt.value= section;
						opt.innerHTML = section;
						select_section_mapbook.appendChild(opt);
					});
				}
			},
			error: function(e){
				console.log("error");
			}
		});			
	});
	
	$('#select_section_report').change(function() {
		var e = document.getElementById('select_site_report');
		var siteStr = e.options[e.selectedIndex].value;
		var site_id = siteStr.split(' (')[1].replace(')', '');
		var section = this.value;

		// get sectionExtent
		getPolygonExtent(site_id, section);
		//

		//
		$("#select_gravesite_report").prop("disabled", true);

		var select_gravesite_report = document.getElementById('select_gravesite_report');
		select_gravesite_report.options.length = 0;
		
		var opt = document.createElement("option");
		opt.value= "Select GRAVESITE";
		opt.innerHTML = "Select GRAVESITE";
		select_gravesite_report.appendChild(opt);

		var url_webservice = webserviceUrl + "/getGraveSiteListFromSection";
		var parameters = "{'site_id':'" + site_id + "','section':'" + section + "'}";

		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				var status = "";
				var jsonData_gravesite = "";
				
				$.each(jsonData, function(item, value){
					if (item == "status"){
						status = value;
					}
					if (item == "gravesiteData"){
						jsonData_gravesite = value;
					}
				});
				if (status == "OK"){
					$.each(jsonData_gravesite, function(item, value){
						var gravesite = value["gravesite"];
						var opt = document.createElement("option");
						opt.value= gravesite;
						opt.innerHTML = gravesite;
						select_gravesite_report.appendChild(opt);
					});
					//
					$("#select_gravesite_report").prop("disabled", false);

				}
			},
			error: function(e){
				console.log("error");
			}
		});			
	});

	$('#select_section_mapbook').change(function() {
		var e = document.getElementById('select_site_mapbook');
		var siteStr = e.options[e.selectedIndex].value;
		var site_id = siteStr.split(' (')[1].replace(')', '');
		var section = this.value;

		// get sectionExtent
		getPolygonExtent(site_id, section);
		//

	});

	$('#select_section_editor').change(function() {
		$('#input_gravesite_poly_objectid').val("");
		$('#select_gravesiteStatus').val("");
		$("label[for='btn_editor_submit']").html("");

		document.getElementById("select_gravesiteStatus").selectedIndex = -1;
		$("#select_gravesiteStatus").prop("disabled", true);
		dijit.byId("btn_editor_submit").setAttribute('disabled', true);
		$("label[for='btn_editor_submit']").html("");		

		var e = document.getElementById('select_site_editor');
		var siteStr = e.options[e.selectedIndex].value;
		var site_id = siteStr.split(' (')[1].replace(')', '');
		var section = this.value;

		//
		$("#select_gravesite_editor").prop("disabled", true);

		var select_gravesite_editor = document.getElementById('select_gravesite_editor');
		select_gravesite_editor.options.length = 0;
		
		var opt = document.createElement("option");
		opt.value= "Select GRAVESITE";
		opt.innerHTML = "Select GRAVESITE";
		select_gravesite_editor.appendChild(opt);

		var url_webservice = webserviceUrl + "/getGraveSiteListFromSection";
		var parameters = "{'site_id':'" + site_id + "','section':'" + section + "'}";

		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				var status = "";
				var jsonData_gravesite = "";
				
				$.each(jsonData, function(item, value){
					if (item == "status"){
						status = value;
					}
					if (item == "gravesiteData"){
						jsonData_gravesite = value;
					}
				});
				if (status == "OK"){
					$.each(jsonData_gravesite, function(item, value){
						var gravesite = value["gravesite"];
						var opt = document.createElement("option");
						opt.value= gravesite;
						opt.innerHTML = gravesite;
						select_gravesite_editor.appendChild(opt);
					});
					//
					$("#select_gravesite_editor").prop("disabled", false);
					// select section 
					if (site_id_current != ""){
						var s = document.getElementById("select_gravesite_editor");
						for ( var i = 0; i < s.options.length; i++ ) {
							if ( s.options[i].value == gravesite_id_current) {
								$("#select_gravesite_editor").val(s.options[i].value).trigger("change");
								break;
							}
						}
					}
				}
			},
			error: function(e){
				console.log("error");
			}
		});			
	});
	
	$('#select_site_search').change(function() {
		$('#select_gravesite_search').empty();
		
		var site_id = this.value.split(' (')[1].replace(')', '');
		
		if (!allUser_status){
			var status = false;
			if (userinfo_array.length > 0){
				for (var i = 0; i < userinfo_array.length; i++)
				{
					var userinfo = userinfo_array[i];
					var site_current = userinfo["site_id"];
					if (site_current == site_id)
					{
						status = true;
						break;
					}
				}
			}
			if (!status){
				var message = "You don't have a permission to this site data(site_id = " + site_id + ").\nPlease contact the site administrator.";
				sendAlert(message);
				return;
			}
		}
		
		// add section
		var select_section_search = document.getElementById('select_section_search');
		select_section_search.options.length = 0;
		
		var opt = document.createElement("option");
		opt.value= "Select SECTION";
		opt.innerHTML = "Select SECTION";
		select_section_search.appendChild(opt);

		opt = document.createElement("option");
		opt.value= "";
		opt.innerHTML = "";
		select_section_search.appendChild(opt);

		var url_webservice = webserviceUrl + "/getSectionListFromSite";
		var parameters = "{'site_id':'" + site_id + "'}";
		//console.log(url_webservice);
		
		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				var status = "";
				var jsonData_section = "";
				
				$.each(jsonData, function(item, value){
					if (item == "status"){
						status = value;
					}
					if (item == "sectionSiteData"){
						jsonData_section = value;
					}
				});
				if (status == "OK"){
					$.each(jsonData_section, function(item, value){
						var section = value["section"];
						var opt = document.createElement("option");
						opt.value= section;
						opt.innerHTML = section;
						select_section_search.appendChild(opt);
					});
				}
			},
			error: function(e){
				console.log("error");
			}
		});			

		// mapping
		var url_site_point_geom = url_service +"/" + layer_sitePoint_index + "/query?where=SITE_ID%3D%27" + site_id + "%27&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&queryByDistance=&returnExtentsOnly=false&datumTransformation=&parameterValues=&rangeValues=&f=pjson";
		
		var url_site_ppolygon_geom = url_site_polygon   + "/query?where=SITE_ID%3D%27" + site_id + "%27&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&queryByDistance=&returnExtentsOnly=false&datumTransformation=&parameterValues=&rangeValues=&f=pjson";
		
		// set definitionExpress
		var queryStr = "SITE_ID = '" + site_id + "' And ACTIVE = 1";
		
		mapImageLayer_cemetery.layerDefinitions[layer_graveSitePoint_index] = queryStr;
		mapImageLayer_cemetery.layerDefinitions[layer_intermentPoint_index] = queryStr;
		mapImageLayer_cemetery.layerDefinitions[layer_gravesitePolygon_index] = queryStr;

		$.getJSON(url_site_ppolygon_geom, function(result){
			var items = [];
			$.each(result, function(item, value){
				if(item == "features"){
					if (value.length > 0){
						for(var j = 0; j < value.length; j++) {
							var geom_json = value[j].geometry;
							var polygon = new Polygon(geom_json);
							polygon.spatialReference = map.spatialReference;
							var polygonExtent = polygon.getExtent().expand(1.1);
							map.setExtent(polygonExtent);
						}
					}
					else{ // use gravesite_marker if cannt get gravesite_poly
						$.getJSON(url_site_point_geom, function(result_1){
							var items = [];
							$.each(result_1, function(item, value){
								if(item == "features"){
									if (value.length > 0){
										for(var j = 0; j < value.length; j++) {
											var geom_json = value[j].geometry;
											var point = new Point(geom_json);
											point.spatialReference = map.spatialReference;
											map.centerAndZoom(point,12);
										}
									}
								}
							});
						});
					}
				}
			});
		});
	});
	
	$('#select_section_search').change(function() {
		
		// site
		var e = document.getElementById('select_site_search');
		var siteStr = e.options[e.selectedIndex].value;
		var site_id = siteStr.split(' (')[1].replace(')', '');
		var section = this.value;

		// gravesite
		$("#select_gravesite").prop("disabled", true);
		//
		var select_gravesite_search = document.getElementById('select_gravesite_search');
		select_gravesite_search.options.length = 0;
		
		var opt = document.createElement("option");
		opt.value= "Select GRAVESITE";
		opt.innerHTML = "Select GRAVESITE";
		select_gravesite_search.appendChild(opt);

		var url_webservice = webserviceUrl + "/getGraveSiteListFromSection";
		var parameters = "{'site_id':'" + site_id + "','section':'" + section + "'}";
		
		gravesiteList_array = [];

		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				//console.log(jsonData);
				var status = "";
				var jsonData_gravesite = "";
				
				$.each(jsonData, function(item, value){
					//console.log(item);
					if (item == "status"){
						status = value;
					}
					if (item == "gravesiteData"){
						jsonData_gravesite = value;
					}
				});
				if (status == "OK"){
					$.each(jsonData_gravesite, function(item, value){
						var gravesite_id = value["gravesite"];
						var splitStr = gravesite_id.split('-');
						var gravesite = splitStr[splitStr.length - 1];
						var jsonStr = {
							gravesite_id: gravesite_id,
							gravesite: gravesite
						}
						gravesiteList_array.push(jsonStr)

						var opt = document.createElement("option");
						opt.value= gravesite_id;
						opt.innerHTML = gravesite_id;
						select_gravesite_search.appendChild(opt);
					});
					//
					$("#select_gravesite").prop("disabled", false);

				}
			},
			error: function(e){
				console.log("error: " + "getGraveSiteListFromSection()...");
			}
		});			
	});

	$('#select_gravesite_search').change(function() {
		var gravesite_id = this.value;
		if (gravesite_id != "Select GRAVESITE")
		{
			var url = url_gravemarker + "/query?where=GRAVEMARKER_ID%3D%27" + gravesite_id + "%27&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=*&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&queryByDistance=&returnExtentsOnly=false&datumTransformation=&parameterValues=&rangeValues=&f=pjson";
			//console.log(url);
			$.getJSON(url, function(result){
				var items = [];
				$.each(result, function(item, value){
					if(item == "features"){
						for(var j = 0; j < value.length; j++) {
							var attributes = value[j].attributes;
							var geom = value[j].geometry;
							addPointToMap(geom, attributes);
						}
					}
				});
			});
		}
	});

	$('#select_gravesite_editor').change(function() {
		$("label[for='btn_editor_submit']").html("");		

		var gravesite_id = this.value;
		if (gravesite_id != "Select GRAVESITE")
		{
			//
			var url_webservice = webserviceUrl + "/getAttributeDataFromGravesite";
			var parameters = "{'gravesite_id':'" + gravesite_id + "'}";
			//console.log(url_webservice);
		
			$.ajax({
				type: "POST",
				url: url_webservice,
				data: parameters,
				contentType: "application/json; charset=utf-8",
				dataType: "json",
				success: function(result) {
					var jsonData = JSON.parse(result.d);
					var status = "";
					var objectid_gravesite_poly = "";
					var gravesite_status = "";
					
					$.each(jsonData, function(item, value){
						if (item == "status"){
							status = value;
						}
						if (item == "objectid_gravesite_poly"){
							objectid_gravesite_poly = value;
						}
						if (item == "gravesite_status"){
							gravesite_status = value;
						}
					});
					if (status == "True"){
						if (objectid_gravesite_poly != ""){
							$('#input_gravesite_poly_objectid').val(objectid_gravesite_poly);
							$('#select_gravesiteStatus').val(gravesite_status);
							//
							$("#select_gravesiteStatus").prop("disabled", false);
							dijit.byId("btn_editor_submit").setAttribute('disabled', false);
						}
					}
				},
				error: function(e){
					console.log("error");
				}
			});			
			
		}
	});

	$('#select_gravesiteStatus').change(function() {
		$("label[for='btn_editor_submit']").html("");		
	});
	
	$('#select_gravesite_people').change(function() {
		var name_people = this.value.split(' = ')[0].trim();
		var gravesite_people = this.value.split(' = ')[1].trim();
		var site_people = gravesite_people.split('-')[0].trim();
		//
		
		// check user permission
		if (!allUser_status){
			var status = false;
			if (userinfo_array.length > 0){
				for (var i = 0; i < userinfo_array.length; i++)
				{
					var userinfo = userinfo_array[i];
					var site_current = userinfo["site_id"];
					if (site_current == site_people)
					{
						status = true;
						break;
					}
				}
			}
			if (!status){
				var message = "You don't have a permission to this site data(site_id = " + site_people + ").\nPlease contact the site administrator.";
				sendAlert(message);
				return;
			}
		}
		
		// check gravesite_id
		var url_webservice = webserviceUrl + "/checkGraveSite";
		var parameters = "{'gravesite_id':'" + gravesite_people + "'}";
		
		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				var statusData = jsonData["status"];
				if (statusData == "True"){
					setMapForGraveSite(site_people, gravesite_people)
				}
				else{
					var message = "<font color='blue'>Cannot find the GraveSite(" + gravesite_people + ") in the GraveSite_Marker_pt layer.\nPlease check the data.</font>";
					sendAlert(message);
					return;
				}
			},
			error: function(e){
				console.log("error: getUserInfo");
				return;
			}
		});
	});
	
	function btn_summaryReport() {
		var e = document.getElementById("select_site_summary");
		var siteStr = e.options[e.selectedIndex].value;
		if (siteStr == "Select SITE")
		{
			alert("Please select a SITE...");
			return;
		}

		var site_id = siteStr.split(' (')[1].replace(')', '');

		// check user permission
		if (!allUser_status){
			var status = false;
			if (userinfo_array.length > 0){
				for (var i = 0; i < userinfo_array.length; i++)
				{
					var userinfo = userinfo_array[i];
					var site_current = userinfo["site_id"];
					if (site_current == site_id)
					{
						status = true;
						break;
					}
				}
			}
			if (!status){
				var message = "You don't have a permission to this site data(site_id = " + site_id + ").\nPlease contact the site administrator.";
				sendAlert(message);
				return;
			}
		}

		// report
		$('#loading_summaryReport').html('<img src="img/ajax-loader.gif"> loading...');

		var sectionStatus = true;
		var url_webservice = webserviceUrl + "/siteSummary";
		var parameters = "{'site_id':'" + site_id + "','sectionStatus':" + sectionStatus + "}";
		
		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var table_main = '<div style="margin-left: 10px;width:95%;height: 170px; overflow: auto;"><table class="layout display responsive-table">';
				var table_section = '';
				
				var jsonData = JSON.parse(result.d);
				var prev_status = "";
				$.each(jsonData, function(item, value){
					if (value["status"] == "section_status"){
						
					}
					else if (value["status"] == "process_status"){
						
					}
					else if (value["status"] == "section"){
						table_section = '<div style="margin-left: 10px;margin-top: 10px;width:95%;height: 320px;; overflow: auto;"><table class="layout display responsive-table">';
						//table_section = '<table class="layout display responsive-table">';
						table_section += '<caption><b>Section Information Summary</b></caption>';
						table_section += '<tr><th>Section</th><th>Type</th><th>Size</th><th>Total</th><th>Obstructed</th><th>Reserved</th><th>Occupied</th><th>Available</th></tr>';

						var jsonData_section = value["sectionData"];
						$.each(jsonData_section, function(item, value){
							var section = value["section"];
							var type = value["type"];
							var size = value["size"];
							var total_count = formatNumber(value["total_count"]);
							var section = value["section"];
							var available_count = formatNumber(value["Available_count"]);
							var obstructed_count = formatNumber(value["Obstructed_count"]);
							var occupied_count = formatNumber(value["Occupied_count"]);
							var reserved_count = formatNumber(value["Reserved_count"]);
							var null_count = formatNumber(value["null_count"]);
							
							table_section += '<tr><td>'+section+'</td><td>'+type+'</td><td>'+size+'</td><td>'+total_count+'</td><td>'+obstructed_count+'</td><td>'+reserved_count+'</td><td>'+occupied_count+'</td><td>'+available_count+'</td></tr>';
							
						});
						table_section += '<table></div>';
						//table_section += '<table>';
						
						//console.log(table_section)
						
						
					}
					else if (value["status"] == "site_id"){
						table_main += '<caption><b>'+value["value"]+' Gravesite Usage Report</b></caption>';
						table_main += '<tr><th style="width: 50px;">Gravesite Status</th><th style="width: 50px;">Type</th><th style="width: 50px;">Count</th>';
					}
					else{
						var status = capitalizeFirstLetter(value["status"]);
						var type = value["type"];
						var value = formatNumber(value["value"]);
						
						if (type == null) type = "";

						if (prev_status != status){
							table_main += '<tr><td><font color="#800000">'+status+'</font></td><td>'+type+'</td></td><td><font color="#800000">'+value+'</font></td></tr>';
						}
						else{
							table_main += '<tr><td>'+''+'</td><td>'+type+'</td></td><td>'+value+'</td></tr>';
						}
						
						prev_status = status;
					}
					
				});
				table_main += '</table></div>';

				var table_data = table_main+table_section;
				var summaryReportResultDiv = document.getElementById("summaryReportResultDiv");
				summaryReportResultDiv.innerHTML = table_data;
				
				//
				$('#loading_summaryReport').html('');

				//
				summaryReportResultDialog.show();
			},
			error: function(e){
				$('#loading_summaryReport').html('');

				sendAlert("<font color='red'>Error: create the repoprt...</font>");
			}
		});			
	};
	
	function formatNumber(numStr) {
		var num = parseInt(numStr) 
		return num.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,")
	}
	
	function btn_gravesiteReport() {
		var e = document.getElementById("select_gravesite_report");
		
		if (e.selectedIndex == -1) {
			alert("Please select a GRAVE SITE...");
			return;
		}
		
		var gravesite_id = e.options[e.selectedIndex].value;
		if (gravesite_id == "Select GRAVE SITE" || gravesite_id == "")
		{
			alert("Please select a GRAVE SITE...");
			return;
		}
		
		$('#loading_gravesiteReport').html('<img src="img/ajax-loader.gif"> loading...');
		
		var site_id = gravesite_id.split('-')[0];
		var section = gravesite_id.split('-')[1];
		var url_webservice = webserviceUrl + "/gravesiteReport";
		var parameters = "{'site_id':'" + site_id + "','section':'" + section + "', 'sectionPolygonExtent':'" + sectionExtent + "','gravesite_id':'" + gravesite_id  + "'}";
		//console.log(parameters);
		
		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				$.each(jsonData, function(item, value) {
					if (value["status"] == "Success") {
						var pdfFileUrl = value["value"];
						$('#loading_gravesiteReport').html('');

						//
						window.open(pdfFileUrl,'_blank');
					}
					else {
						$('#loading_gravesiteReport').html('');
						sendAlert("<font color='blue'>Warning: cannot create the report.</font>");
					}
				});
			},
			error: function(e){
				console.log("error");
				$('#loading_gravesiteReport').html('');
				sendAlert("<font color='red'>Error: create the repoprt.</font>");
			}
		});			
	}

	function btn_editor_submit() {
		var e = document.getElementById("select_gravesiteStatus");
		if (e.selectedIndex < 0){
			sendAlert("Please select a Gravesite_status...");
			return;
		}

		//
		var gravesite_poly_objectid = document.getElementById("input_gravesite_poly_objectid").value;
		var gravesite_status = e.options[e.selectedIndex].value;
		var url_webservice = webserviceUrl + "/updateAttributesForGravesitePoly";
		var userName_tmp = stringEscape(currentUser);
		var parameters = "{'objectid_gravesite_poly':'" + gravesite_poly_objectid + "', 'gravesite_status':'" + gravesite_status + "', 'userName':'" + userName_tmp + "'}";
		var status = "";
		//console.log(parameters);
		
		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				$.each(jsonData, function(item, value){
					if (item == "status"){
						status = value;
					}
				});
				if (status == "True"){
					status = "success";
				}
				else{
					status = "warning";
				}
				$("label[for='btn_editor_submit']").html(status);

				// refresh map
				for(var j = 0; j < map.layerIds.length; j++) {
					var layer_tmp = map.getLayer(map.layerIds[j]);
					if (layer_tmp.arcgisProps["title"].indexOf(cemeteryServiceName) >= 0){
						layer_tmp.refresh();
						break;
					}
				}
				//
			},
			error: function(e){
				status = "error";
				$("label[for='btn_editor_submit']").html(status);		
			}
		});
	}
	
	function btn_mapbookReport() {
		var e1 = document.getElementById("select_site_mapbook");
		var e2 = document.getElementById("select_section_mapbook");
		var e3 = document.getElementById("email_id");
		
		if (e1.selectedIndex <= 0 || e2.selectedIndex <= 0 || e3.value == ""){
			sendAlert("Please select a SITE, SECTION and Email...");
			return;
		}
		//if (sectionExtent == ""){
			//sendAlert("Warning: cannot find SECTION polygon geometry...");
			//return;
		//}
		var siteStr = e1.options[e1.selectedIndex].value;
		var site_id = siteStr.split(' (')[1].replace(')', '');
		var section = e2.options[e2.selectedIndex].value;
		var email = e3.value;
		var url_webservice = webserviceUrl + "/createMapBookReport";
		var parameters = "{'site_id':'" + site_id + "','section':'" + section + "', 'sectionPolygonExtent':'" + sectionExtent + "','email':'" + email  + "'}";

		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			async: true,
			contentType: "application/json; charset=utf-8",
			dataType: "json"
		});
		
		// close dialogbox
		mapbookReportDialog.hide();
		
	};
	
	function btn_peopleSubmit() {
		//$("#select_site_people").empty();
		$("#select_gravesite_people").empty();
		
		//var select_site_people = document.getElementById('select_site_people');
		var select_gravesite_people = document.getElementById('select_gravesite_people');
		
		var opt_gravesite_people = document.createElement("option");
		opt_gravesite_people.value= "Select GRAVESITE";
		opt_gravesite_people.innerHTML = "Select GRAVESITE";
		select_gravesite_people.appendChild(opt_gravesite_people);
		
		//
		var e_firstname = document.getElementById('firstname_field');
		var e_lastname = document.getElementById('lastname_field');
		
		var firstName = e_firstname.value;
		var lastName = e_lastname.value;

		if (firstName == "Select FIRST NAME" && lastName == "Select LAST NAME"){
			alert("Please select either FIRSTNAME or LASTNAME or BOTH...");
			return;
		}
		
		if (firstName == "Select FIRST NAME") firstName = "";
		if (lastName == "Select LAST NAME") lastName = "";

		//
		var url_webservice = webserviceUrl + "/getIntermentInfo";
		var parameters = "{'first_name':'" + firstName + "','last_name':'" + lastName + "'}";
					
		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				var status = jsonData["status"];
				if (status == "OK"){
					var siteList = jsonData["siteList"];
					var intermentList = jsonData["intermentList"];
					
					//
					for (var i = 0; i < intermentList.length; i++)
					{
						var interment = intermentList[i];
						var site_id = interment["site_id"];
						var gravesite_id = interment["gravesite_id"];
						var name = interment["name"];
						
						opt_gravesite_people = document.createElement("option");
						opt_gravesite_people.value= name + " = " + gravesite_id;
						opt_gravesite_people.innerHTML = name + " = " + gravesite_id;
						select_gravesite_people.appendChild(opt_gravesite_people);
					}
				}
				
			},
			error: function(e){
				console.log("error: getIntermentNameList_firstName");
			}
		});			
	};

	function getPolygonExtent(site_id, section){
		sectionExtent = "";
		
		var url_geom = url_section_polygon   + "/query?where=site_id%3D%27" + site_id + "%27+and+section+%3D+%27" + section + "%27&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&queryByDistance=&returnExtentsOnly=false&datumTransformation=&parameterValues=&rangeValues=&f=pjson";
		$.getJSON(url_geom, function(result){
			var items = [];
			$.each(result, function(item, value){
				if(item == "features"){
					for(var j = 0; j < value.length; j++) {
						var geom_json = value[j].geometry;
						var polygon = new Polygon(geom_json);
						polygon.spatialReference = map.spatialReference;
						var sectionPolygonExtent = polygon.getExtent().expand(1.1);
						var xmin = sectionPolygonExtent["xmin"];
						var ymin = sectionPolygonExtent["ymin"];
						var xmax = sectionPolygonExtent["xmax"];
						var ymax = sectionPolygonExtent["ymax"];
						
						sectionExtent = xmin + ", " + ymin + ", " + xmax + ", " + ymax;
						//console.log(url_geom);
						//console.log("sectionExtent", site_id, section, sectionExtent);
					}
				}
			});
		});
	}
	
	function getUserName() {
		userName = "";
		var url_webservice = webserviceUrl + "/userName";
		var parameters = "{}";
		
		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				var status = jsonData["status"];
				var userName = jsonData["value"];
				$('#login').val(userName);
				
				currentUser = userName;
				//
				getUserInfo(userName);
				//
				getBookmarks(userName);
			},
			error: function(e){
				console.log("error");
				$('#login').val("");
			}
		});			

	}

	function getIntermentNameList(site_id) {
		firstName_array = [];
		lastName_array = [];
		
		var url_webservice = webserviceUrl + "/getNameList";
		var parameters_firstname = "{'nameType':'" + "FIRSTNAME" + "', 'site_id':'" + site_id + "'}";
		var parameters_lastname =  "{'nameType':'" +  "LASTNAME" + "', 'site_id':'" + site_id + "'}";

		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters_firstname,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				var status = jsonData["status"];
				if (status == "OK"){
					var firstNameList = jsonData["firstNameList"];
					for (var i = 0; i < firstNameList.length; i++)
					{
						var firstName = firstNameList[i];
						firstName_array.push(firstName);
					}
				}
				
			},
			error: function(e){
				console.log("error: getIntermentNameList_firstName");
			}
		});			

		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters_lastname,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				var status = jsonData["status"];
				if (status == "OK"){
					var lastNameList = jsonData["lastNameList"];
					for (var i = 0; i < lastNameList.length; i++)
					{
						var lastName = lastNameList[i];
						lastName_array.push(lastName);
					}
				}
			},
			error: function(e){
				console.log("error: getIntermentNameList_lastName");
			}
		});			
		
	}

	function getUserInfo(userName) {
		userName = stringEscape(userName);
		var url_webservice = webserviceUrl + "/getUserPermissionInfo";
		var parameters = "{'userName':'" + userName + "'}";

		var mapbookStatus = false;
		editorStatus = false;
		var email = "";
		
		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				var status = jsonData["status"];
				if (status == "OK"){
					var userinfoList = jsonData["userinfoList"];
					for (var i = 0; i < userinfoList.length; i++)
					{
						var userInfo = userinfoList[i];
						var mapbook = userInfo["mapbook"];
						var editor = userInfo["editor"];
						email = userInfo["email"];
						
						if (mapbook ==1){
							mapbookStatus = true;
						}
						if (editor ==1){
							editorStatus = true;
						}
						userInfo_array.push(userInfo);
					}
				}
				//
				if (mapbookStatus){
					var btn = dijit.byId('mapbookReport_btn').domNode;
					dojo.style(btn, {visibility:'visible'});
					//
					$('#email_id').val(email);
				}
				//
				if (editorStatus){
					var btn = dijit.byId('editor_btn').domNode;
					dojo.style(btn, {visibility:'visible'});
				}

				//console.log("mapbook", mapbookStatus);
			},
			error: function(e){
				console.log("error: getUserInfo");
			}
		});			
	}
	
	function stringEscape(s) {
		return s ? s.replace(/\\/g,'\\\\').replace(/\n/g,'\\n').replace(/\t/g,'\\t').replace(/\v/g,'\\v').replace(/'/g,"\\'").replace(/"/g,'\\"').replace(/[\x00-\x1F\x80-\x9F]/g,hex) : s;
		function hex(c) { var v = '0'+c.charCodeAt(0).toString(16); return '\\x'+v.substr(v.length-2); }
	}
	
	function setMapForGraveSite(site, gravesite){
		// set definitionExpress
		var queryStr = "SITE_ID = '" + site + "' And Active = 1";
		//console.log("1", quertStr);
		mapImageLayer_cemetery.layerDefinitions[layer_graveSitePoint_index] = queryStr;
		mapImageLayer_cemetery.layerDefinitions[layer_intermentPoint_index] = queryStr;
		mapImageLayer_cemetery.layerDefinitions[layer_gravesitePolygon_index] = queryStr;
		
		var url = url_gravemarker + "/query?where=GRAVEMARKER_ID%3D%27" + gravesite + "%27&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=*&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&queryByDistance=&returnExtentsOnly=false&datumTransformation=&parameterValues=&rangeValues=&f=pjson";
		
		$.getJSON(url, function(result){
			var items = [];
			$.each(result, function(item, value){
				if(item == "features"){
					for(var j = 0; j < value.length; j++) {
						var attributes = value[j].attributes;
						var geom = value[j].geometry;
						addPointToMap(geom, attributes);
					}
				}
			});
		});
	}
	
	function populateAutoComplete() {
		$("#firstname_field").smartAutoComplete({source: firstName_array, forceSelect: true, maxResults: 5, delay: 200 });
		$("#lastname_field").smartAutoComplete({source: lastName_array, forceSelect: true, maxResults: 5, delay: 200 });
	}
	
	function capitalizeFirstLetter(string) {
		return string.charAt(0).toUpperCase() + string.slice(1);
	}
	
	function sendAlert(message) {
		var message_data = "<p style='width: 400px;height: 200px;'>" + message + "</p>";
		
		var attributeDialog = new dijit.Dialog({
			title: "Message",
			content: message_data,
			style: "width: 450px; height: 200px"
		});
		attributeDialog.show();
	}
	
	function checkGraveSite(gravesite_id) {
		var url_webservice = webserviceUrl + "/checkGraveSite";
		var parameters = "{'gravesite_id':'" + gravesite_id + "'}";
		
		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				var statusData = jsonData["status"];
				if (statusData == "True"){
					return true;
				}
			},
			error: function(e){
				console.log("error: getUserInfo");
				return false
			}
		});
	}

	function populateSiteList(){
		var select_site_search = document.getElementById('select_site_search');
		var select_site_summary = document.getElementById('select_site_summary');
		var select_site_report = document.getElementById('select_site_report');
		var select_site_mapbook = document.getElementById('select_site_mapbook');
		var select_site_editor = document.getElementById('select_site_editor');
		var select_site_people = document.getElementById('select_site_people');


		var opt = document.createElement("option");
		opt.value= "Select SITE";
		opt.innerHTML = "Select SITE";

		var opt1 = document.createElement("option");
		opt1.value= "Select SITE";
		opt1.innerHTML = "Select SITE";

		var opt2 = document.createElement("option");
		opt2.value= "Select SITE";
		opt2.innerHTML = "Select SITE";

		var opt3 = document.createElement("option");
		opt3.value= "Select SITE";
		opt3.innerHTML = "Select SITE";

		var opt4 = document.createElement("option");
		opt4.value= "Select SITE";
		opt4.innerHTML = "Select SITE";

		var opt5 = document.createElement("option");
		opt5.value= "Select SITE";
		opt5.innerHTML = "Select SITE";

		select_site_search.appendChild(opt);
		select_site_summary.appendChild(opt1);
		select_site_report.appendChild(opt2);
		select_site_mapbook.appendChild(opt3);
		select_site_people.appendChild(opt4);
		select_site_editor.appendChild(opt5);

		opt4 = document.createElement("option");
		opt4.value= "";
		opt4.innerHTML = "";
		select_site_people.appendChild(opt4);

		//
		var url_webservice = webserviceUrl + "/getSiteList";
		var parameters = "";

		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
				var status = jsonData["status"];
				if (status == "OK"){
					var siteList = jsonData["siteData"];
					
					// get user's siteList
					if (!allUser_status){
						var siteList_user = [];
						if (userinfo_array.length > 0){
							for (var i = 0; i < userinfo_array.length; i++)
							{
								var userinfo = userinfo_array[i];
								var site_current = userinfo["site_id"];
								siteList_user.push(site_current);
							}
						}
						siteList = siteList_user;
					}
					siteList_current = siteList;
					
					var siteList_items = "";
					for (var i = 0; i < siteList.length; i++)
					{
						var siteItem = siteList[i];
						var site_id = siteItem["site_id"];
						var name = siteItem["name"];
						var site = name+" ("+site_id+")";
						
						if (!allUser_status){
							if(i == 0){
								siteList_items = "'"+site_id+"'";
							}
							else{
								siteList_items += ",'"+site_id+"'";
							}
						}
						//
						opt = document.createElement("option");
						opt.value= site;
						opt.innerHTML = site;
						//
						opt1 = document.createElement("option");
						opt1.value= site;
						opt1.innerHTML = site;
						//
						opt2 = document.createElement("option");
						opt2.value= site;
						opt2.innerHTML = site;
						//
						opt3 = document.createElement("option");
						opt3.value= site;
						opt3.innerHTML = site;
						//
						opt4 = document.createElement("option");
						opt4.value= site;
						opt4.innerHTML = site;
						//
						opt5 = document.createElement("option");
						opt5.value= site;
						opt5.innerHTML = site;
						
						select_site_search.appendChild(opt);
						select_site_summary.appendChild(opt1);
						select_site_report.appendChild(opt2);
						select_site_mapbook.appendChild(opt3);
						select_site_people.appendChild(opt4);
						select_site_editor.appendChild(opt5);
						
					}
					//
					if (!allUser_status){
						var queryStr = "SITE_ID in ("+siteList_items+")";
						mapImageLayer_cemetery.layerDefinitions[layer_sitePoint_index] = queryStr;
						console.log("siteList_user");
						console.log(queryStr);
					}
				}
			},
			error: function(e){
				console.log("error: getSiteList");
			}
		});			
		select_site_search.selectedIndex = 0;
		select_site_summary.selectedIndex = 0;
		select_site_report.selectedIndex = 0;
		select_site_mapbook.selectedIndex = 0;
		select_site_people.selectedIndex = 0;
		


	}

	function getBookmarks(userName){
		//
		if (currentUser == ""){
			return;
		}

		userName = stringEscape(userName);
		var url_webservice = webserviceUrl + "/getBookmarks";
		var parameters = "{'user_name':'" + userName + "'}";

		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);

				var bookmarkList = [];
				if (jsonData.length > 0){
					for (var i = 0; i < jsonData.length; i++){
						var bookmark_str = jsonData[i];
						var splitStr = bookmark_str.split(";");
						var name =  splitStr[0];
						var spatialreference = Number(splitStr[1]);
						var box = splitStr[2];
						splitStr = box.split(',');
						var xmin = Number(splitStr[0]);
						var ymin = Number(splitStr[1]);
						var xmax = Number(splitStr[2]);
						var ymax = Number(splitStr[3]);
						var extent_json = {
							'spatialReference': {"wkid": spatialreference},
							'xmax': xmax,
							'xmin': xmin,
							'ymax': ymax,
							'ymin': ymin
						}
						var bookmark_json = {
							'extent': extent_json,
							'name': name
						}
						bookmarkList.push(bookmark_json);
					}
				}
				//console.log(bookmarkList);
				
				if ( bookmarkList.length > 0) {
					for (var i = 0; i < bookmarkList.length; i++){
						var b = bookmarkList[i];
						bookmark.addBookmark(b);
					}
				}

			},
			error: function(e){
				console.log("error: getBookmarks");
			}
		});			

	}

	function getDomainList(){
		// layer_graveSitePoint_index
		var url_gravesite_marker_domain = url_service + "/queryDomains?layers=%5B" + layer_graveSitePoint_index + "%5D&f=pjson";
		var url_gravesite_marker = url_service + "/" + layer_graveSitePoint_index + "?f=pjson";

		$.getJSON(url_gravesite_marker, function(result){
			$.each(result, function(item, value){
				//
				if (item == 'fields'){
					for (var i = 0; i < value.length; i++){
						var field = value[i];
						var fieldName = field["name"];
						var fieldDomain = field["domain"];
						
						if (fieldDomain != null){
							//console.log(fieldName, fieldDomain["codedValues"]);
							var domain_json = {
								'name': fieldName,
								'codedValues': fieldDomain["codedValues"],
							};
							domainList_gravesite_marker.push(domain_json);
						}
					}
				}
			});
		});

		// layer_sitePoint_index
		var url_site_point_domain = url_service + "/queryDomains?layers=%5B" + layer_sitePoint_index + "%5D&f=pjson";
		var url_site_point = url_service + "/" + layer_sitePoint_index + "?f=pjson";

		//console.log(url_site_point);
		$.getJSON(url_site_point, function(result){
			$.each(result, function(item, value){
				//
				if (item == 'fields'){
					for (var i = 0; i < value.length; i++){
						var field = value[i];
						var fieldName = field["name"];
						var fieldDomain = field["domain"];
						
						if (fieldDomain != null){
							var domain_json = {
								'name': fieldName,
								'codedValues': fieldDomain["codedValues"],
							};
							domainList_site_point.push(domain_json);
						}
					}
				}
			});
		});
		
		// layer_gravesitePolygon_index
		$("#select_gravesiteStatus").prop("disabled", true);
		dijit.byId("btn_editor_submit").setAttribute('disabled', true);


		$('#select_gravesiteStatus').empty();

		var select_gravesiteStatus = document.getElementById('select_gravesiteStatus');
		
		var url_gravesite_marker_domain = url_service + "/queryDomains?layers=%5B" + layer_gravesitePolygon_index + "%5D&f=pjson";
		var url_gravesite_marker = url_service + "/" + layer_gravesitePolygon_index + "?f=pjson";

		$.getJSON(url_gravesite_marker, function(result){
			$.each(result, function(item, value){
				//
				var domainList = [];
				if (item == 'fields'){
					for (var i = 0; i < value.length; i++){
						var field = value[i];
						var fieldName = field["name"];
						var fieldDomain = field["domain"];
						
						if (fieldDomain != null){
							if (fieldName == "GRAVESITE_STATUS"){
								domainList = fieldDomain["codedValues"];
								if (domainList.length > 0){
									for (var n = 0; n < domainList.length; n++){
										var opt = document.createElement("option");
										opt.value= domainList[n]["code"];
										opt.innerHTML = domainList[n]["code"];

										select_gravesiteStatus.appendChild(opt);
										select_gravesiteStatus.selectedIndex = -1;
									}
								}
								break;
							}
						}
					}
				}
			});
		});
	}
	
	function addPointToMap(geom_json, values){
		var point = new Point(geom_json);
		point.spatialReference = map.spatialReference;
		
		gLayer.add(new Graphic(point, infoSymbol));
		map.centerAndZoom(point,24);

		//
		var table_data = '<div style="width:98%;height:98%; overflow: auto;"><table class="layout display responsive-table">';
		var front_image = "";
		var rear_image = "";
		var interment_photo_1 = "";
		var interment_photo_2 = "";
		var no_image = "img/not-available.jpg";

		site_id_current = "";
		section_current = "";
		gravesite_id_current = "";
		
		// editor
		if (editorStatus){
			var editor_data = '<button id="toEditor_btn" onClick="editorDialog.show();" data-dojo-type="dijit/form/Button" type="button" showLabel="false" style="margin: 5px;background-color: transparent;">Go to Editor</button>';
			table_data = editor_data + '<div style="margin-top: 5px;width:98%;height:98%; overflow: auto;"><table class="layout display responsive-table">';
		}
		else{
			table_data = '<div style="width:98%;height:98%; overflow: auto;"><table class="layout display responsive-table">';
		}
		
		$.each(values, function(fieldName, fieldValue){
			var fieldNameStr = fieldName.toUpperCase();
			var arrayIndex = $.inArray(fieldNameStr, fields_notUse);
			
			if (fieldValue == "Null"){
				fieldValue = "";
			}
			if (fieldValue == null){
				fieldValue = "";
			}
			
			// get domsin value
			var domainList = "";
			for (var i = 0; i < domainList_gravesite_marker.length; i++){
				if (domainList_gravesite_marker[i]["name"] == fieldName){
					domainList = domainList_gravesite_marker[i];
					break;
				}
			}
			
			if (domainList.length != ""){
				var codedValues = domainList["codedValues"];
				for (var i = 0; i < codedValues.length; i++){
					var domainValue_name = codedValues[i]["name"];
					var domainValue_code = codedValues[i]["code"];
					if (domainValue_code == fieldValue){
						fieldValue = domainValue_name;
					}
				}
			}
			
			if (arrayIndex == -1)
			{
				table_data += '<tr><td>' + fieldName.replace('_', ' ') + '</td><td>' + fieldValue + '</td></tr>'
				
				//
				if (editorStatus){
					if (fieldName == "SITE_ID"){
						site_id_current = fieldValue;
					}
					if (fieldName == "GRAVEMARKER_ID"){
						gravesite_id_current = fieldValue;
					}
					if (fieldName == "SECTION"){
						section_current = fieldValue;
					}
				}
				
				
				if (fieldName == "FRONT_IMG"){

					if (fieldValue != null) front_image = fieldValue;
				}
				if (fieldName == "REAR_IMG"){
					if (fieldValue != null) rear_image = fieldValue;
				}
				if (fieldName == "IMG1"){
					if (fieldValue != null) interment_photo_1 = fieldValue;
				}
				if (fieldName == "IMG2"){
					if (fieldValue != null) interment_photo_2 = fieldValue;
				}
			}
		});
		
		if (front_image == "Null") front_image = no_image;
		if (rear_image == "Null") rear_image = no_image;
		if (interment_photo_1 == "Null") interment_photo_1 = no_image;
		if (interment_photo_2 == "Null") interment_photo_2 = no_image;

		if (front_image.trim() == "") front_image = no_image;
		if (rear_image.trim() == "") rear_image = no_image;
		if (interment_photo_1.trim() == "") interment_photo_1 = no_image;
		if (interment_photo_2.trim() == "") interment_photo_2 = no_image;
		
		if (front_image.trim() == "http://cemquaweb5/cemPhotos/noImage.jpg") front_image = no_image;
		if (rear_image.trim() == "http://cemquaweb5/cemPhotos/noImage.jpg") rear_image = no_image;
		if (interment_photo_1.trim() == "http://cemquaweb5/cemPhotos/noImage.jpg") interment_photo_1 = no_image;
		if (interment_photo_2.trim() == "http://cemquaweb5/cemPhotos/noImage.jpg") interment_photo_2 = no_image;
		
		if (front_image != no_image) front_image = photoUrl + site_id_current + "/marker/" + front_image;
		if (rear_image != no_image) rear_image = photoUrl + site_id_current + "/marker/" + rear_image;
		if (interment_photo_1 != no_image) interment_photo_1 = photoUrl + site_id_current + "/marker/" + interment_photo_1;
		if (interment_photo_2 != no_image) interment_photo_2 = photoUrl + site_id_current + "/marker/" + interment_photo_2;
		

		//
		table_data += '</Table>';
		photoList = [];
		
		// front_photo
		var table_front_photo = '';
		if (front_image.trim() != ""){
			photoList.push("Front Photo" + "|" + front_image)
			table_front_photo = '<br><table class="layout display responsive-table"><caption>Front Photo</caption>';
			table_front_photo += "<tr><td><img src='" + front_image + "' alt='' class='center' onclick='openImage(\"" + front_image + "\");'" + "></td></tr>";
			table_front_photo += '</Table>';
		}

		// rear photo
		var table_rear_photo = '';
		if (rear_image.trim() != ""){
			photoList.push("Rear Photo" + "|" + rear_image)
			table_rear_photo = '<br><table class="layout display responsive-table"><caption>Rear Photo</caption>';
			table_rear_photo += "<tr><td><img src='" + rear_image + "' alt='' class='center' onclick='openImage(\"" + rear_image + "\");'" + "></td></tr>";
			table_rear_photo += '</Table>';
		}

		// interment photo-1
		var table_interment_photo_1 = '';
		if (interment_photo_1.trim() != ""){
			photoList.push("Interment Photo-1" + "|" + interment_photo_1)
			table_interment_photo_1 = '<br><table class="layout display responsive-table"><caption>Interment Photo-1</caption>';
			table_interment_photo_1 += "<tr><td><img src='" + interment_photo_1 + "' alt='' class='center' onclick='openImage(\"" + interment_photo_1 + "\");'" + "></td></tr>";
			table_interment_photo_1 += '</Table>';
		}
		
		// interment photo-2
		var table_interment_photo_2 = '';
		if (interment_photo_2.trim() != ""){
			photoList.push("Interment Photo-2" + "|" + interment_photo_2)
			table_interment_photo_2 = '<br><table class="layout display responsive-table"><caption>Interment Photo-2</caption>';
			table_interment_photo_2 += "<tr><td><img src='" + interment_photo_2 + "' alt='' class='center' onclick='openImage(\"" + interment_photo_2 + "\");'" + "></td></tr>";
			table_interment_photo_2 += '</Table>';
		}
		
		table_data += table_front_photo;
		table_data += table_rear_photo;
		table_data += table_interment_photo_1;
		table_data += table_interment_photo_2;

		table_data += '</div>'
		
		// popup
		document.getElementById("identifyResultDiv").innerHTML = table_data;
		identifyResultDialog.set('title', "Gravesite Information"); 
		identifyResultDialog.show();

		// populate editor dialog
		if (editorStatus){
			var s = document.getElementById("select_site_editor");
			for ( var i = 0; i < s.options.length; i++ ) {
				if ( s.options[i].value.indexOf("("+site_id_current+")") != -1) {
					$("#select_site_editor").val(s.options[i].value).trigger("change");
					//
					//$("#select_section_editor").val(section).trigger("change");
					//$("#select_gravesite_editor").val(gravesite_id).trigger("change");
					//
					break;
				}
			}
			//console.log("editor", site_id, section, gravesite_id);

		}
		
		
		//
		map.setMapCursor("default");
	}

	
	function addPointToMap_site(geom_json, values){
		var point = new Point(geom_json);
		point.spatialReference = map.spatialReference;
		
		gLayer.add(new Graphic(point, infoSymbol));
		//map.centerAndZoom(point,24);

		//
		var table_data = '<div style="width:98%;height:98%; overflow: auto;"><table class="layout display responsive-table">';
		var site_id = "";
		var short_name = "";
		
		$.each(values, function(fieldName, fieldValue){
			if (fieldValue == "Null"){
				fieldValue = "";
			}
			if (fieldValue == "null"){
				fieldValue = "";
			}
			if (fieldName == "OBJECTID") fieldName = "";
			if (fieldName == "") fieldName = "";
			if (fieldName == "POSITION_SRC") fieldName = "";
			if (fieldName == "ACTIVED") fieldName = "";
			if (fieldName == "GlobalID") fieldName = "";
			if (fieldName == "created_user") fieldName = "";
			if (fieldName == "created_date") fieldName = "";
			if (fieldName == "last_edited_user") fieldName = "";
			if (fieldName == "last_edited_date") fieldName = "";
			if (fieldName == "SHAPE") fieldName = "";
			if (fieldName == "ACTIVE") fieldName = "";

			if (fieldName == "Website_URL"){
				fieldValue = "<a href='" + fieldValue + "' target='_blank'>" + "Cemetery Webpage" + "</a>";
			}
			if (fieldName == "SITE_ID") site_id = fieldValue;
			if (fieldName == "SHORT_NAME") short_name = fieldValue.replace(' ', '').replace('.', '');

			fieldName = fieldName.replace('_', ' ');
			
			// get domsin value
			var domainList = "";
			for (var i = 0; i < domainList_site_point.length; i++){
				if (domainList_site_point[i]["name"] == fieldName){
					domainList = domainList_site_point[i];
					break;
				}
			}
			
			if (domainList.length != ""){
				var codedValues = domainList["codedValues"];
				for (var i = 0; i < codedValues.length; i++){
					var domainValue_name = codedValues[i]["name"];
					var domainValue_code = codedValues[i]["code"];
					if (domainValue_code == fieldValue){
						fieldValue = domainValue_name;
					}
				}
			}
			
			if (fieldName != ""){	
				table_data += '<tr><td>' + fieldName + '</td><td>' + fieldValue + '</td></tr>'
			}
		});
		var hyperlink = "<a href='" + databookUrl + "/" + short_name + site_id + ".asp' target='_blank'>Visit DataBook</a>";
		table_data += '<tr><td>' + "DataBook" + '</td><td>' + hyperlink + '</td></tr>'
		
		//
		table_data += '</table></div>';

		// popup
		document.getElementById("identifyResultDiv").innerHTML = table_data;
		identifyResultDialog.set('title', "Cemetery Site Information"); 
		identifyResultDialog.show();
		//
		map.setMapCursor("default");
	}
	
	function refreshBookmarks() {
		if ( useLocalStorage ) {
		  window.localStorage.setItem(storageName, dojo.toJson(bookmark.toJson()));
		} else {
		  var exp = 7; // number of days to persist the cookie
		  dojo.cookie(storageName, dojo.toJson(bookmark.toJson()), { 
			expires: exp
		  });
		}
	}
	
	function clearBookmarks() {
		var conf = confirm('Click OK to remove your map bookmarks.');
		if ( conf ) {
		  if ( useLocalStorage ) {
			// Remove from local storage
			window.localStorage.removeItem(storageName);
		  } else {
			// Remove cookie
			dojo.cookie(storageName, null, { expires: -1 });
		  }
		  // Remove all user defined bookmarks
		  // First get all bookmark names
		  var bmNames = dojo.map(bookmark.bookmarks, function(bm) {
			if ( bm.name != 'Central Pennsylvania' ) {
			  return bm.name;
			}
		  });
		  // Run removeBookmark
		  dojo.forEach(bmNames, function(bName) {
			bookmark.removeBookmark(bName);
		  });
		  alert('Bookmarks Removed.');
		}
	}
	
	function supports_local_storage() {
		try {
			return 'localStorage' in window && window['localStorage'] !== null;
		} catch( e ){
			return false;
		}
	}

	function identifyGravesiteMarker (event) {
		try {
			var scale = scaleUtils.getScale(map);
			map.setMapCursor("wait");
			gLayer.clear();

			if (scale <= gravesiteScale){
				if (mapImageLayer_cemetery.layerInfos[layer_graveSitePoint_index].defaultVisibility && mapImageLayer_cemetery.layerInfos[layer_gravesitePolygon_index].defaultVisibility){
					identifyParams.geometry = event.mapPoint;
					identifyParams.mapExtent = map.extent;
					identifyTask.execute(identifyParams, function (idResults) {
						if (idResults.length > 0){
							var idResult = idResults[0];
							var feat = idResult.feature;
							var geom = feat.geometry;
							var attributes = feat.attributes;
							addPointToMap(geom, attributes);
							//
							map.setMapCursor("default");
						}
						else{
							console.log("not find gravesite marker and try the gravesite polygon");
							//
							identifyParams_gravesite_polygon.geometry = event.mapPoint;
							identifyParams_gravesite_polygon.mapExtent = map.extent;
							identifyTask_gravesite_polygon.execute(identifyParams_gravesite_polygon, function (idResults) {
								if (idResults.length > 0){
									var idResult = idResults[0];
									var feat = idResult.feature;
									var geom = feat.geometry;
									var attributes = feat.attributes;
									
									var gravesite_id = "";
									$.each(attributes, function(fieldName, fieldValue){
										if (fieldName == "GRAVESITE_ID"){
											gravesite_id = fieldValue;
										}
									});

									if (gravesite_id != ""){
										var url = url_gravemarker + "/query?where=GRAVEMARKER_ID%3D%27" + gravesite_id + "%27&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=*&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&queryByDistance=&returnExtentsOnly=false&datumTransformation=&parameterValues=&rangeValues=&f=pjson";
										$.getJSON(url, function(result){
											var items = [];
											$.each(result, function(item, value){
												if(item == "features"){
													for(var j = 0; j < value.length; j++) {
														var attributes = value[j].attributes;
														var geom = value[j].geometry;
														addPointToMap(geom, attributes);
														//
														map.setMapCursor("default");
													}
												}
											});
										});
									}
								}
								else{
									map.setMapCursor("default");
								}
							});
						}
					});
				}
				else{
					map.setMapCursor("default");
				}
			}
			else{
				if (mapImageLayer_cemetery.layerInfos[layer_sitePoint_index].defaultVisibility){
					console.log("layer_sitePoint_index", layer_sitePoint_index);
					identifyParams_site.geometry = event.mapPoint;
					identifyParams_site.mapExtent = map.extent;
					identifyTask_site.execute(identifyParams_site, function (idResults) {
						if (idResults.length > 0){
							var idResult = idResults[0];
							var feat = idResult.feature;
							var geom = feat.geometry;
							var attributes = feat.attributes;
							addPointToMap_site(geom, attributes);
							//
							map.setMapCursor("default");
						}
						else{
							map.setMapCursor("default");
						}
					});
				}
				else{
					map.setMapCursor("default");
				}
			}
		}
		catch(err) {
			map.setMapCursor("default");
		}
	}

	function sortJson(element, prop, propType, asc) {
	  switch (propType) {
		case "int":
		  element = element.sort(function (a, b) {
			if (asc) {
			  return (parseInt(a[prop]) > parseInt(b[prop])) ? 1 : ((parseInt(a[prop]) < parseInt(b[prop])) ? -1 : 0);
			} else {
			  return (parseInt(b[prop]) > parseInt(a[prop])) ? 1 : ((parseInt(b[prop]) < parseInt(a[prop])) ? -1 : 0);
			}
		  });
		  break;
		default:
		  element = element.sort(function (a, b) {
			if (asc) {
			  return (a[prop].toLowerCase() > b[prop].toLowerCase()) ? 1 : ((a[prop].toLowerCase() < b[prop].toLowerCase()) ? -1 : 0);
			} else {
			  return (b[prop].toLowerCase() > a[prop].toLowerCase()) ? 1 : ((b[prop].toLowerCase() < a[prop].toLowerCase()) ? -1 : 0);
			}
		  });
	  }
	}	

	bookmarkDialog.connect(bookmarkDialog, "hide", function(e){
		//console.log("bookmarkDialog_close");
		var bookmarks_json = bookmark["bookmarks"];

		var bookmarks_str = "";
		if (bookmarks_json.length > 0){
			for (var i = 0; i < bookmarks_json.length; i++){
				if (i == 0){
					var name = bookmarks_json[i]["name"];
					var extent = bookmarks_json[i]["extent"];
					var spatialReference = extent["spatialReference"]["wkid"];
					var xmin = extent["xmin"];
					var ymin = extent["ymin"];
					var xmax = extent["xmax"];
					var ymax = extent["ymax"];
					var extent_str = xmin+","+ymin+","+xmax+","+ymax;
					var json = name+";"+spatialReference+";"+extent_str;
					bookmarks_str = json;
				}
				else{
					var name = bookmarks_json[i]["name"];
					var extent = bookmarks_json[i]["extent"];
					var spatialReference = extent["spatialReference"]["wkid"];
					var xmin = extent["xmin"];
					var ymin = extent["ymin"];
					var xmax = extent["xmax"];
					var ymax = extent["ymax"];
					var extent_str = xmin+","+ymin+","+xmax+","+ymax;
					var json = name+";"+spatialReference+";"+extent_str;

					bookmarks_str += "|" + json;
				}
			}
		}
		
		//console.log("bookmarks_str", bookmarks_str);
		userName = stringEscape(currentUser);
		var url_webservice = webserviceUrl + "/storeBookmarks";
		var parameters = "{'user_name':'" + userName + "','bookmarks_str':'" + bookmarks_str + "'}";

		$.ajax({
			type: "POST",
			url: url_webservice,
			data: parameters,
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function(result) {
				var jsonData = JSON.parse(result.d);
			},
			error: function(e){
				console.log("error: storeBookmarks");
			}
		});			

	});

	// dojo ready
	//dojo.ready(init);
	
	// document ready
	$(document).ready(function(){
		// turn off buttons
		var btn_mapbookRepport = dijit.byId('mapbookReport_btn').domNode;
		dojo.style(btn_mapbookRepport, {visibility:'hidden'});

		var btn_editor = dijit.byId('editor_btn').domNode;
		dojo.style(btn_editor, {visibility:'hidden'});

		//
		init();
		
		console.log("Document Ready...");
	});
	

});

