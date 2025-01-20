/* Author: Mattias Bengtsson, Insytec AB (2012) */

$(document).ready(function() {
	// Language/site selector dialog
	$(".fancybox").fancybox({ padding: 0 });

	// Account-menu toggle
	$("#account-user a").click(function() {
		$("#account").toggleClass("open");
		return false;
	});

	// Position megamenu arrow below the current menu-item
	$("header nav ul > li > a").mouseenter(function() {
		var megamenu = $(this).next();
		var nav = megamenu.closest("nav");
		if (nav.length == 0)
			return;
		var navLeft = nav.position().left;
		var menuItemLeft = $(this).parent().position().left;
		var menuItemWidth = $(this).parent().outerWidth();
		var megamenuWidth = megamenu.outerWidth();
		var megamenuLeft = ($("header").width() - megamenuWidth) / 2 - navLeft;
		if (menuItemLeft < megamenuLeft)
			megamenuLeft = menuItemLeft;
		megamenu.css("left", megamenuLeft);
		var arrow = megamenu.find(".megamenu-arrow");
		var arrowWidth = arrow.outerWidth();
		var arrowLeft = menuItemLeft - megamenuLeft + (menuItemWidth / 2) - (arrowWidth / 2);
		arrow.css("left", arrowLeft);
	});

	// Search inputs
	$(".search-input").each(function() {
		var label = $(this).find("label");
		var input = $(this).find("input");
		var link = input.next();

		// Placeholder text
		var placeholderText = label.text();
		if (input.val() == "")
			input.val(placeholderText);
		input.focus(function() {
			if (input.val() == placeholderText)
				input.val("");
		}).blur(function() {
			if (input.val() == "")
				input.val(placeholderText);
		});

		// Submit on enter
		input.keydown(function(e) {
			if (e.keyCode == 13) {
				link.trigger("click");
				return false;
			}
		});

		// Submit on click
		link.click(function() {
			var query = input.val();
			if (query == placeholderText)
				return false;
			var url = link.attr("href");
			if (query.length > 0)
				url += (url.indexOf("?") > -1 ? "&" : "?") + "query=" + encodeURIComponent(query);
			window.location.href = url;
			return false;
		});
	});

	// Page print
	$("#action-print a").click(function() {
		if ($(this).attr("href") != "#") {
			$(this).attr("target", "_blank");
			return true;
		}
		window.print();
		return false;
	});

	// Share and feedback form
	$(".action-form input").focus(function() {
		if ($(this).hasClass("placeholder"))
			$(this).val("");
		$(this).removeClass("placeholder");
	});
	$("#feedback-type").change(function() {
		$("#feedback-address-region").toggle($(this).val() == "catalogue");
	});
	$(".action-form-send a").click(function() {
		// Validate
		var form = $(this).closest(".action-form");
		var isValid = validateForm(form);
		if (isValid) {
			// Send
			form.find(".action-form-input").hide();
			var postUrl = document.location.pathname + document.location.search;
			var postData = form.find(":input").not(".placeholder").serialize();
			var jqxhr = $.post(
				postUrl,
				postData,
				function(data) {
					if (data == "ok") {
						form.find(".action-form-success").show();
						setTimeout(function() { $.fancybox.close(); }, 3000);
					} else
						alert("The form could not be sent, sorry.");
				})
			.error(function () {
				alert("Something went wrong, sorry.");
			});
		}
		return false;
	});

	// Order catalog links
	$("a[href=#ordercatalogue]").click(function() {
		$.fancybox("#feedback", { padding: 0 });
		$("#feedback-type").val("catalogue").trigger("change");
		$("#feedback-message").val($(this).attr("title"));
		return false;
	});

	// Links to open feedback (with specified type preselected)
	$("a.feedback").click(function() {
		$.fancybox("#feedback", { padding: 0 });
		$("#feedback-type").val($(this).attr("href").replace("#", "")).trigger("change");
		$("#feedback-message").val($(this).attr("title"));
		return false;
	});

	// Carousel
	var carouselStandardLink = $("#carousel-image a").attr("href");
	var carouselStandardImg = $("#carousel-image img").attr("src");
	var carouselStandardExists = !!carouselStandardLink;
	var switchCarouselSlide = function(index, animate) {
		$("#carousel-pager a").removeClass("active");
		var slidePager = $("#carousel-pager a:eq(" + index + ")");
		slidePager.addClass("active");
		var imgUrl = index == null ? carouselStandardImg : slidePager.data("img");
		if (animate) {
			$("<img>").prop("src", imgUrl).hide().appendTo("#carousel-image a").fadeIn(
				400,
				function() {
					$(this).prevAll().remove();
				}
			);
		} else
			$("#carousel-image img").attr("src", imgUrl);
		$("#carousel-image a").attr("href", index == null ? carouselStandardLink : slidePager.attr("href"));
		$("#carousel-indicator div").removeClass("active");
		$("#carousel-indicator div:eq(" + index + ")").addClass("active");
	};
	if (!carouselStandardExists) {
		$("<a><img></a>").appendTo("#carousel-image");
		switchCarouselSlide(0, false);
	}

	// Automatically switch carousel images
	var windowWidth = $(window).width();
	var isMobile = windowWidth < 1000;
	var carouselAutoSwitch = !isMobile;
	var carouselSlide = carouselStandardExists ? 0 : 1;
	var carouselInitialDelay = 15;
	var carouselSlideInterval = 6;
	if ($("#carousel").length) {
		var slideCount = $("#carousel-pager a").length;
		window.setTimeout(function() {
			window.setInterval(function() {
				if (!carouselAutoSwitch)
					return;
				var isLastSlide = carouselSlide >= slideCount;
				if (isLastSlide) {
					carouselSlide = 0;
					if (carouselStandardExists)
						switchCarouselSlide(null, true);
					else
						switchCarouselSlide(carouselSlide++, true);
				} else
					switchCarouselSlide(carouselSlide++, true);
			}, carouselSlideInterval * 1000);
		}, Math.max((carouselInitialDelay - carouselSlideInterval) * 1000, 0));
	}

	// Switch carousel image when hovering the pages buttons
	$("#carousel-pager a").hover(function() {
		carouselSlide = $(this).prevAll().length;
		switchCarouselSlide(carouselSlide, false);
	});

	// Pause carousel slideshow when hovering the image or the buttons
	$("#carousel-image, #carousel-pager a").hover(function() {
		carouselAutoSwitch = false;
	}, function() {
		carouselAutoSwitch = true;
	});

	// Startpage products
	var startProdStandardImg = $("#start-products img").attr("src");
	$("#start-products a").hover(function() {
		$("#start-products img").attr("src", $(this).data("img"));
	}, function() {
		$("#start-products img").attr("src", startProdStandardImg);
	});

	// Startpage carousel-button-dialogs
	var openStartDialog = function(dialog, button) {
		$(".action-form").not(dialog).hide();
		dialog.toggle();
		var buttonPosition = button.offset();
		var buttonWidth = button.width();
		var buttonHeight = button.outerHeight();
		var dialogWidth = dialog.outerWidth();
		var dialogOffsetTop = 8;
		var dialogOffsetLeft = -29;
		var windowWidth = $(window).width();
		if (dialogWidth > windowWidth) {
			dialogWidth = windowWidth;
			dialog.css("width", dialogWidth);
		}
		var dialogLeft = buttonPosition.left - dialogWidth / 2 + buttonWidth / 2 + dialogOffsetLeft;
		if (dialogLeft + dialogWidth > windowWidth)
			dialogLeft = windowWidth - dialogWidth - 6;
		if (dialogLeft < 0)
			dialogLeft = 0;
		dialog.css({ top: buttonPosition.top + buttonHeight - dialogOffsetTop, left: dialogLeft });
	};
	$("div#carousel-button-contactus").click(function() {
		openStartDialog($("#start-contact"), $(this));
		return false;
	});
	$("div#carousel-button-newsletter").click(function() {
		openStartDialog($("#start-newsletter"), $(this));
		return false;
	});

	// Multicontent tab-switch
	$(".multicontent .tabs li").click(function() {
		var mc = $(this).closest(".multicontent");
		mc.find(".tabs li.active, .content.active").removeClass("active");
		$(this).addClass("active");
		var currentTab = $(this).prevAll().length;
		mc.find(".content:eq(" + currentTab + ")").addClass("active");
	});
	$(".multicontent .tabs li:first-child, .multicontent .content:eq(0)").addClass("active");

	// Hide popups when clicking outside
	var runOnClickOutside = function(event, element, callback) {
		if (element.length == 1 && event.target.id !== element[0].id && !$.contains(element[0], event.target))
			callback(element);
	};
	$(document.body).click(function(e) {
		// Account menu
		runOnClickOutside(e, $("#account-menu"), function(element) { element.parent().removeClass("open"); });

		// Search as-you-type
		runOnClickOutside(e, $("#earch-popup"), function(element) { element.hide(); });

		// Startpage dialogs
		runOnClickOutside(e, $("#start-contact"), function(element) { element.hide(); });
		runOnClickOutside(e, $("#start-newsletter"), function(element) { element.hide(); });
	});

	// Search as-you-type helpers
	var isNavigateKey = function(keyCode) {
		return
		    keyCode == 13 /*enter*/ ||
		    keyCode == 16 /*shift*/ ||
		    keyCode == 17 /*ctrl*/ ||
		    keyCode == 18 /*alt*/ ||
		    keyCode == 35 /*end*/ ||
		    keyCode == 36 /*home*/ ||
		    keyCode == 37 /*left arrow*/ ||
		    keyCode == 38 /*up arrow*/ ||
		    keyCode == 39 /*right arrow*/ ||
		    keyCode == 40 /*down arrow*/;
	};
	var saytMinQueryLength = 2;
	var saytExecute = null;
	var saytKeyDelay = 300;

	// Site search (search as-you-type)
	var siteSearchUrl = $("#search-asyoutype-url").val();
	$("#search input").keyup(function(e) {
		// Don't do anything if you're not changing values
		if (isNavigateKey(e.keyCode))
			return;

		// Reset if there is not enough letters
		var query = $(this).val();
		if (query.length <= saytMinQueryLength) {
			$("#search-popup").hide();
			return;
		}

		// Update as-you-type
		clearTimeout(saytExecute);
		saytExecute = setTimeout(function() {
			var jqxhr = $.get(siteSearchUrl, { query: query }, function(data) {
				$("#search-popup").toggle(query.length > saytMinQueryLength - 1);
				$("#search-popup-content").html(data);
			})
			.error(function(jqXHR, textStatus, errorThrown) {
				$("#search-popup").hide();
				console.log("Search as-you-type error");
			});
		}, saytKeyDelay);
	});

	// Page form
	$(".pageform .graphbutton a").click(function() {
		// Validate
		var isValid = validateForm($(".pageform"));
		if (isValid) {
			$(this).parent().addClass("disabled");
			// Send
			var postForm = $("<form>").attr("action", $(this).attr("href")).attr("method", "post").attr("enctype", "multipart/form-data").css("display", "none");
			$(".pageform input, textarea").clone().appendTo(postForm);
			$(".pageform select").each(function() {
				$("<input>").attr("name", $(this).attr("name")).attr("value", $(this).val()).appendTo(postForm);
			});
			postForm.appendTo("body");
			postForm.submit();
		}
		return false;
	});
	// Page form: submit on enter
	$(".pageform input:last").keyup(function(e) {
		if (e.keyCode == 13) {
			$(".pageform a").trigger("click");
			return false;
		}
	});

	// Page form: switch radio-toggle inputs
	$(".pageform .radiotoggle input:radio").change(function() {
		var checked = $(this).is(":checked");
		var current = $(this).parent().next();
		$(".radiotoggle-content").not(current).slideUp();
		current.slideDown();
	});

	// Page form: open radio-toggle if preselected
	$(".pageform .radiotoggle input:radio").each(function() {
		if ($(this).is(":checked"))
			$(this).parent().next().show();
	});

	// Load prices/atps
	var priceAtp = [];
	$(".itempatp").each(function() {
		priceAtp.push($(this).attr("id").split("-")[1]);
	});
	if (priceAtp.length > 0) {
		var updateStock = $("#stock-legend").length > 0;
		var updatePrice = $(".product-items th.numeric").length > 0;
		if (!updateStock && !updatePrice)
			return;
		var i, j, priceAtpBatch, batchSize = 25;
		for (i = 0, j = priceAtp.length; i < j; i += batchSize) {
			priceAtpBatch = priceAtp.slice(i, i + batchSize);
			$.ajaxq("atpqueue", {
				url: $("#priceatp-url").val(),
				dataType: "json",
				data: { items: priceAtpBatch.join(";") },
				cache: false,
				success: function(data) {
					$.each(data, function(i) {
						var itemRow = $("#item-" + data[i].id);
						if (updateStock)
							itemRow.find("td:first-child").addClass("stock-" + data[i].stock);
						if (updatePrice)
							itemRow.find("td:last-child").text(data[i].price);
					});
				},
				error: function(event, request, settings) {
					console.log("Ajax request error");
				}
			});
		}
	}

	// Validate quantity
	var validateQty = function() {
		var qty = parseInt($("#add-qty").val());
		var errorLabel = $("#item-addorder .form-error");
		errorLabel.hide();
		if (qty) {
			$("#add-qty").val(qty);
			return qty;
		}
		errorLabel.show();
		return 0;
	};

	// Add to cart
	$("#item-addorder-button a").click(function() {
		// Validate quantity
		var qty = validateQty();
		if (qty == 0)
			return false;

		// Send
		var button = $(this).parent();
		if (button.hasClass("disabled"))
			return false;
		button.addClass("disabled");
		var postUrl = $(this).attr("href");
		var postData = $("#add-item, #add-qty").serialize() + "&returncount=1";
		var jqxhr = $.post(
			postUrl,
			postData,
			function(data) {
				$("#add-qty").val("");
				button.removeClass("disabled");
				$("#item-addorder-message").slideToggle();
				setTimeout(function() { $("#item-addorder-message").slideToggle(); }, 3000);
				$("#shoplinks-cart span").html(data);
			})
		.error(function() {
			alert("Something went wrong, sorry.");
		});
		return false;
	});

	// Check availability
	$("#item-checkavail-button a").click(function() {
		$("#item-checkavail").remove();

		// Validate quantity
		var qty = validateQty();
		if (qty == 0)
			return false;

		// Send
		var button = $(this).parent();
		if (button.hasClass("disabled"))
			return false;
		button.addClass("disabled");
		var jqxhr = $.get($(this).attr("href"), { item: $("#add-item").val(), qty: qty }, function(data) {
			$("#item-checkavail-button").after(data);
		})
		.complete(function(jqXHR, textStatus) {
			button.removeClass("disabled");
		})
		.error(function(jqXHR, textStatus, errorThrown) {
			console.log("Check availability error: " + jqXHR.status);
		});
		return false;
	});

	// Add order-history articles to cart
	$("#order-details .graphbutton a").click(function() {
		// Get values
		var collection = [];
		$("#order-details > table tr:not(tr:first-child)").each(function() {
			collection.push({ name: $(this).find("a").text(), value: parseInt($(this).find("td.numerical:eq(0)").text()) });
		});
		if (collection.length == 0)
			return false;

		// Post to cart
		var postForm = $("<form>").attr("action", $(this).attr("href")).attr("method", "post").css("display", "none");
		$("<input>").attr("name", "quickorder").attr("value", $.param(collection)).appendTo(postForm);
		postForm.appendTo("body");
		postForm.submit();
		return false;
	});

	// Check customer-name on activate user
	var erpCustomerCheckTimeout = null;
	$(".pageform .checkval").keyup(function(e) {
		var input = $(this);
		if (input.next().hasClass("checkval-response"))
			input.next().remove();
		if (!input.val())
			return;
		clearTimeout(erpCustomerCheckTimeout);
		erpCustomerCheckTimeout = setTimeout(function() {
			$.getJSON($("#checkval-url").val(), { customer: input.val() }, function(data) {
				$("<p>").addClass("checkval-response").text(data.name).insertAfter(input);
			})
			.error(function(event, request, settings) {
				console.log("Ajax request error");
			});
		}, 1000);
	});

	// Select all in download center
	$("#download-center-hits th:first-child").click(function() {
		$("#download-center-selectall").trigger("click");
	});
	$("#download-center-selectall").click(function() {
		var state = $(this).data("state") == "allchecked";
		$(this).data("state", state ? "nonechecked" : "allchecked");
		$("#download-center-hits input").prop("checked", !state);
		if ($(this).data("seltext") == null)
			$(this).data("seltext", $(this).text());
		$(this).text($(this).data(state ? "seltext" : "deseltext"));
	});

	// Download center hero search
	$(".download-center-hero-search button").click(function() {
		var querystring = $(this).parent().find("input, select").filter(function() { return $(this).val(); }).serialize();
		var url = $(this).parent().data("url");
		url += (url.indexOf("?") > -1 ? "&" : "?") + querystring;
		window.location.href = url;
		return false;
	});

	// Download center hero update hitcounts as-you-type
	var dcSearchUrl = $(".download-center-hero-search").data("updatehitsurl");
	$("#download-center-query").keyup(function(e) {
		// Don't do anything if you're not changing values
		if (isNavigateKey(e.keyCode))
			return;

		// Hide search-results
		$("#download-center-hits").hide();

		// Reset if there is not enough letters
		var query = $(this).val();
		if (query.length <= saytMinQueryLength) {
			$("#download-center-type option").each(function(i) {
				if ($(this).attr("value").length > 0)
					$(this).text($(this).attr("value"));
				else
					$(this).text($(this).data("text"));
			});
			$("#download-center-filters :checkbox").each(function(i) {
				if ($(this).attr("value").length > 0)
					$(this).next("span").text($(this).attr("value"));
				else
					$(this).next("span").text($(this).data("text"));
			});
			return;
		}

		// Update as-you-type
		clearTimeout(saytExecute);
		saytExecute = setTimeout(function() {
			var jqxhr = $.getJSON(dcSearchUrl, { query: query }, function(data) {
				$.each(data, function(index, value) {
					if (value.resourcetype == null) {
						var filterOptionAll = $("#download-center-type option[value='']");
						filterOptionAll.text(filterOptionAll.data("text") + " (" + value.hits + ")");
						var filterCheckboxAll = $("#download-center-filter-all");
						filterCheckboxAll.next("span").text(filterCheckboxAll.data("text") + " (" + value.hits + ")");
					} else {
						var filterOption = $("#download-center-type option[value='" + value.resourcetype + "']");
						filterOption.text(filterOption.attr("value") + " (" + value.hits + ")");
						var filterCheckbox = $("#download-center-filters :checkbox[value='" + value.resourcetype + "']");
						filterCheckbox.next("span").text(filterCheckbox.attr("value") + " (" + value.hits + ")");
					}
				});
			})
			.error(function(event, request, settings) {
				console.log("Ajax request error");
			});
		}, saytKeyDelay);
	});

	// Download center filters
	$("#download-center-filters :checkbox").click(function() {
		$(this).closest("li").toggleClass("active", $(this).is(":checked"));

		var allFilterCheckbox = $("#download-center-filter-all");
		if ($(this).is(allFilterCheckbox)) {
			$("#download-center-filters :checkbox").not(allFilterCheckbox).prop("checked", false).closest("li").removeClass("active");
		} else {
			allFilterCheckbox.prop("checked", false).closest("li").removeClass("active");
			if ($(this).attr("value") == "CAD drawing" && !$(this).is(":checked")) {
				$(this).closest("li").find(":checkbox").prop("checked", false).closest("li").removeClass("active");
			}
		}

		var url = "index.html";
		var query = $("#download-center-query").val();
		if (query.length > 0)
			url += (url.indexOf("?") > -1 ? "&" : "?") + "query=" + encodeURIComponent(query);
		var filterQs = $("#download-center-filters input").filter(function() { return $(this).val(); }).serialize();
		if (filterQs.length > 0)
			url += (url.indexOf("?") > -1 ? "&" : "?") + filterQs;
		document.location = url;
	});

	// Contact browser
	var toggleOption = function(option, show) {
		if (show)
			option.filter("span > option").unwrap();
		else
			option.filter(":not(span > option)").wrap("<span>").parent().hide();
	};
	var displayContactCountries = function() {
		var region = $("#contact-region").val();
		$(".contact-browser-country").toggle(region != null);
		$("#contact-country option")
			.each(function(i) {
				toggleOption($(this), i == 0 || $(this).data("region") == region);
			});
	};
	$("#contact-region").change(function() {
		displayContactCountries();
		$("#contact-country option").prop("selected", false).first().prop("selected", true);
	});
	displayContactCountries();
	var goToContactCountry = function() {
		var url = $("#contact-country option:selected").val();
		if (url.length)
			document.location = url;
	};
	$("#contact-country").change(function() {
		goToContactCountry();
	});
	$(".contact-browser-country .graphbutton a").click(function() {
		goToContactCountry();
		return false;
	});

	// Google Analytics event: click contact info email
	$(".event-contactinfo a").click(function() {
		if ($(this).attr("href").substring(0, 7) == "mailto:")
			sendGaEvent("Mail", "Info", $(this).attr("href").replace("mailto:", ""));
	});
	// Google Analytics event: click organisation email
	$(".event-contactorg").click(function() {
		sendGaEvent("Mail", "Organisation", $(this).attr("href").replace("mailto:", ""));
	});
	// Google Analytics event: click person email
	$(".event-contactperson").click(function() {
		sendGaEvent("Mail", "Contacts", $(this).attr("href").replace("mailto:", ""));
	});
	// Google Analytics event: send startpage contact form
	$("#start-contact .action-form-send a").click(function() {
		var formIsValid = !$(this).parent().hasClass("disabled");
		if (formIsValid)
			sendGaEvent("ContactForm", "Send", $("#feedback-type").val());
	});
	// Google Analytics event: send startpage newsletter form
	$("#start-newsletter .action-form-send a").click(function() {
		var formIsValid = !$(this).parent().hasClass("disabled");
		if (formIsValid)
			sendGaEvent("Newsletter", "Send");
	});
	// Google Analytics event: click asset in download center
	$(".event-downloadfile").click(function() {
		sendGaEvent("Download", "File", $(this).attr("href"));
	});
	// Google Analytics event: click asset in product details
	$(".event-productfile").click(function() {
		sendGaEvent("Download", "Productdetails", $(this).data("eventlabel"));
	});

	// Mobile (responsive) menu
	$(".mobile-menu-toggle").click(function() {
		$("#mobile-menu-wrapper").toggleClass("opened");
	});
	$(document).mouseup(function(e) {
		var container = $("#mobile-menu-wrapper");
		if (!container.is(e.target) && container.has(e.target).length === 0)
			container.removeClass("opened");
	});

	// Activate date-picker
	$(".datepicker").each(function(index) {
		var isoFieldId = "#" + $(this).data("datepickerisofield");
		var isoField = $(isoFieldId);
		$(this).datepicker({ altField: isoFieldId, altFormat: "yy-mm-dd" });
		if (isoField.val())
			$(this).datepicker("setDate", parseDateISO8601(isoField.val()));
	});
});

// Wrap Google Analytics-function to avoid exceptions it when it is not included
function sendGaEvent(category, action, label) {
	if (typeof ga != "function") {
		console.log("Send GA event (category: " + category + ", action: " + action + ", label: " + label + ")");
		return;
	}
	ga("send", "event", category, action, label);
};

function validateForm(form) {
	var isValid = true;

	// Revalidate when changing the form after the first validation
	if (!form.data("validated")) {
		form.find("input[type=text], input[type=password], textarea").on("input change", function() {
			validateForm(form);
		});
		form.find(":radio, :checkbox, select, input[type=file]").change(function() {
			validateForm(form);
		});
	}
	form.data("validated", true);

	// Clear error-messages
	form.find(".form-error, .form-error-header").hide();
	form.find(".graphbutton").removeClass("disabled");

	// Validate
	var fields = form.find(":input:visible");
	var emailRegex = /^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$/i;
	fields.filter(".validate-email").each(function(i) {
		if ($(this).val().length > 0 && !emailRegex.test($(this).val())) {
			$(this).parent().find(".form-error").show();
			isValid = false;
		}
	});
	fields.filter(".validate-multipleemail").each(function(i) {
		var emails = $(this).val().replace(" ", "").replace(";", ",").split(",");
		$.each(emails, function() {
			isValid = isValid && emailRegex.test(this);
		});
		if (!isValid) {
			$(this).parent().find(".form-error").show();
		}
	});
	fields.filter(".validate-mandatory").each(function(i) {
		if (($(this).attr("type") == "checkbox" && !$(this).is(":checked")) ||
			!$(this).val()) {
			$(this).parent().find(".form-error").show();
			isValid = false;
		}
	});
	fields.filter(".validate-minlength").each(function(i) {
		var minLength = 0;
		$.each($(this).attr("class").split(" "), function() {
			if (this.indexOf("length-") == 0)
				minLength = parseInt(this.split("-")[1]);
		});
		if ($(this).val().length < minLength) {
			$(this).parent().find(".form-error").show();
			isValid = false;
		}
	});
	var numericRegex = /^\d*$/;
	fields.filter(".validate-numeric").each(function(i) {
		if (!numericRegex.test($(this).val())) {
			$(this).parent().find(".form-error").show();
			isValid = false;
		}
	});
	var passwordRegex = /(?=^.{8,}$)(?=.*[A-Z])(?=.*[a-z]).*$/;
	fields.filter(".validate-password").each(function(i) {
		if ($(this).val() && !passwordRegex.test($(this).val())) {
			$(this).parent().find(".form-error").show();
			isValid = false;
		}
	});
	fields.filter(".validate-repeat").each(function(i) {
		if ($(this).val() != $("#" + $(this).attr("id").replace("-repeat", "")).val()) {
			$(this).parent().find(".form-error").show();
			isValid = false;
		}
	});
	var phoneRegex = /^\+[\d -]+$/;
	fields.filter(".validate-phone").each(function(i) {
		if (!phoneRegex.test($(this).val())) {
			$(this).parent().find(".form-error").show();
			isValid = false;
		}
	});

	// Done
	form.find(".form-error-header").toggle(!isValid);
	if (!isValid)
		form.find(".graphbutton").addClass("disabled");
	return isValid;
}

// IE doesn't parse ISO-dates, use custom function
function parseDateISO8601(dateString) {
	var isoExp = /^\s*(\d{4})-(\d\d)-(\d\d)\s*$/,
		date = new Date(NaN),
		month,
		parts = isoExp.exec(dateString);
	if (parts) {
		month = +parts[2];
		date.setFullYear(parts[1], month - 1, parts[3]);
		if (month != date.getMonth() + 1) {
			date.setTime(NaN);
		}
	}
	return date;
}
