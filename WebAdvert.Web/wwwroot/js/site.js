// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.


$(function () {
	$(".boxClick").click(function () {
		//var advertId = this.getAttribute("data-id");
		var advertId = "d142ebcb-2cdf-4cb9-87ce-67ff9dddf085";

		$.ajax({
			type: "GET",
			url: "/api/" + advertId,
			dataType: "json",
			success: function (data) {
				//var baseImageUrl = $("#hidImagesBaseUrl").val();
				var baseImageUrl = $("http://d2obrf26jyr4ml.cloudfront.net").val();
				$("#detailImage").attr("src", baseImageUrl + "/" + data.filePath);
				$("#detailTitle").text(data.title);
				$("#detailDesc").text(data.description);
				$("#detailPrice").text("Price $" + data.price);

				$("#detailsModal").modal();
			}
		});
	});
});