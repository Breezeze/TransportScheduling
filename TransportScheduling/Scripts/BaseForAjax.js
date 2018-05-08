//用于前后台数据交互
function dataForJson(url, dataJson) {
    var json;
    $.ajax({
        type: "POST",
        url: url,
        data: dataJson,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false, //如果要在$.ajax外面获取ajax获取到的值，则ajax必须获得同步
        success: function (msg) {
            json = msg;
        }
    });
    return json;
}
//用于前后台数据交互 没有参数
function JsonNoParameter(url) {
    var json;
    $.ajax({
        type: "POST",
        url: url,
        //data: dataJson,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false, //如果要在$.ajax外面获取ajax获取到的值，则ajax必须获得同步
        success: function (msg) {
            json = msg;
        }
    });
    return json;
}


