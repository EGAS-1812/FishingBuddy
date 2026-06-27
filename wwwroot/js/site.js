(function () {
	function debounce(callback, delay) {
		var timeoutId;
		return function () {
			var args = arguments;
			var context = this;
			clearTimeout(timeoutId);
			timeoutId = setTimeout(function () {
				callback.apply(context, args);
			}, delay);
		};
	}

	function escapeHtml(value) {
		return value
			.replaceAll("&", "&amp;")
			.replaceAll("<", "&lt;")
			.replaceAll(">", "&gt;")
			.replaceAll('"', "&quot;")
			.replaceAll("'", "&#39;");
	}

	function parseLocaleDateTime(value, locale) {
		var normalized = (value || "").trim();
		if (!normalized) {
			return null;
		}

		var hrPattern = /^(\d{1,2})[.\-/](\d{1,2})[.\-/](\d{4})(?:\s+(\d{1,2}):(\d{2}))?$/;
		var enPattern = /^(\d{1,2})[\/\-.](\d{1,2})[\/\-.](\d{4})(?:\s+(\d{1,2}):(\d{2}))?$/;

		var match;
		var day;
		var month;
		var year;
		var hour = 0;
		var minute = 0;

		if (locale.startsWith("hr") && hrPattern.test(normalized)) {
			match = normalized.match(hrPattern);
			day = Number(match[1]);
			month = Number(match[2]);
			year = Number(match[3]);
			hour = Number(match[4] || 0);
			minute = Number(match[5] || 0);
		} else if (enPattern.test(normalized)) {
			match = normalized.match(enPattern);
			if (locale.startsWith("hr")) {
				day = Number(match[1]);
				month = Number(match[2]);
			} else {
				month = Number(match[1]);
				day = Number(match[2]);
			}
			year = Number(match[3]);
			hour = Number(match[4] || 0);
			minute = Number(match[5] || 0);
		} else {
			var fallback = new Date(normalized);
			return Number.isNaN(fallback.getTime()) ? null : fallback;
		}

		var date = new Date(year, month - 1, day, hour, minute, 0, 0);
		return Number.isNaN(date.getTime()) ? null : date;
	}

	function formatLocaleDateTime(date, locale) {
		var options = {
			day: "2-digit",
			month: "2-digit",
			year: "numeric",
			hour: "2-digit",
			minute: "2-digit",
			hour12: false
		};
		return new Intl.DateTimeFormat(locale, options).format(date);
	}

	function toLocalIsoString(date) {
		var pad = function (value) {
			return String(value).padStart(2, "0");
		};

		return (
			date.getFullYear() +
			"-" +
			pad(date.getMonth() + 1) +
			"-" +
			pad(date.getDate()) +
			"T" +
			pad(date.getHours()) +
			":" +
			pad(date.getMinutes()) +
			":00"
		);
	}

	function prefersReducedMotion() {
		return window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
	}

	function animateRows(rows) {
		var reducedMotion = prefersReducedMotion();

		rows.forEach(function (row, index) {
			row.classList.remove("fb-row-enter");
			row.classList.remove("fb-row-reveal");

			if (reducedMotion) {
				row.classList.add("fb-row-updated");
				setTimeout(function () {
					row.classList.remove("fb-row-updated");
				}, 500);
				return;
			}

			row.style.animationDelay = (index * 90) + "ms";
			void row.offsetWidth;
			row.classList.add("fb-row-enter", "fb-row-updated");
			setTimeout(function () {
				row.classList.remove("fb-row-updated");
				row.style.animationDelay = "0ms";
			}, 1500);
		});
	}

	function setSearchLoadingState(container, status, isLoading) {
		container.classList.toggle("fb-refreshing", isLoading);

		if (status) {
			status.classList.toggle("is-loading", isLoading);
		}
	}

	function initAjaxSearchTables() {
		var searchContainers = document.querySelectorAll("[data-ajax-search]");

		searchContainers.forEach(function (container) {
			var endpoint = container.dataset.searchEndpoint;
			var mode = container.dataset.searchMode || "index";
			var input = container.querySelector("[data-search-input]");
			var status = container.querySelector("[data-search-status]");
			var target = container.querySelector("[data-search-target]");
			var emptyMessage = container.dataset.emptyMessage || "Nema rezultata.";
			var emptyColspan = Number(container.dataset.emptyColspan || 1);
			var activeController;

			if (!endpoint || !input || !target) {
				return;
			}

			var runSearch = debounce(function () {
				var term = input.value.trim();
				if (activeController) {
					activeController.abort();
				}

				activeController = new AbortController();
				setSearchLoadingState(container, status, true);
				if (status) {
					status.textContent = term ? "Pretrazujem..." : "Prikaz svih zapisa.";
				}

				var query = new URLSearchParams();
				query.set("term", term);
				query.set("mode", mode);

				fetch(endpoint + "?" + query.toString(), {
					headers: {
						"X-Requested-With": "XMLHttpRequest"
					},
					signal: activeController.signal
				})
					.then(function (response) {
						if (!response.ok) {
							throw new Error("Search request failed.");
						}
						return response.text();
					})
					.then(function (html) {
						target.innerHTML = html.trim();

						var rows = Array.from(target.querySelectorAll("tr"));
						if (rows.length === 0) {
							target.innerHTML =
								'<tr><td colspan="' +
								emptyColspan +
								'" class="text-center py-4 text-muted">' +
								escapeHtml(emptyMessage) +
								"</td></tr>";
						} else {
							animateRows(rows);
						}

						if (status) {
							status.textContent = term
								? "Rezultati osvjezeni za pojam: " + term
								: "Prikaz svih zapisa.";
						}
					})
					.catch(function (error) {
						if (error.name === "AbortError") {
							return;
						}

						if (status) {
							status.textContent = "Pretrazivanje trenutno nije dostupno.";
						}
					})
					.finally(function () {
						setSearchLoadingState(container, status, false);
					});
			}, 280);

			input.addEventListener("input", runSearch);
		});
	}

	function initPageLoadReveal() {
		var revealTargets = document.querySelectorAll(
			".card, .table-responsive, [data-ajax-search], .fb-list-search, .fb-home-search-panel, .fb-white-pill-card"
		);
		var reducedMotion = prefersReducedMotion();

		revealTargets.forEach(function (element, index) {
			element.classList.add("fb-reveal-on-load");
			element.style.setProperty("--fb-reveal-delay", Math.min(index * 65, 560) + "ms");

			if (reducedMotion) {
				element.classList.add("is-visible");
				return;
			}

			requestAnimationFrame(function () {
				element.classList.add("is-visible");
			});
		});
	}

	function initAutocompleteDropdowns() {
		var controls = document.querySelectorAll("[data-autocomplete]");

		controls.forEach(function (control) {
			var endpoint = control.dataset.endpoint;
			var hiddenInput = control.querySelector("[data-autocomplete-value]");
			var textInput = control.querySelector("[data-autocomplete-text]");
			var list = control.querySelector("[data-autocomplete-list]");
			var status = control.querySelector("[data-autocomplete-status]");
			var placeholder = control.dataset.placeholder || "Odaberite";
			var selectedIndex = -1;
			var items = [];
			var activeController;

			if (!endpoint || !hiddenInput || !textInput || !list) {
				return;
			}

			function closeList() {
				control.classList.remove("is-open");
				textInput.setAttribute("aria-expanded", "false");
				selectedIndex = -1;
			}

			function render() {
				list.innerHTML = "";

				if (items.length === 0) {
					list.innerHTML = '<li class="fb-autocomplete-empty" role="option" aria-disabled="true">Nema rezultata.</li>';
					control.classList.add("is-open");
					textInput.setAttribute("aria-expanded", "true");
					return;
				}

				items.forEach(function (item, index) {
					var option = document.createElement("li");
					option.className = "fb-autocomplete-item";
					option.setAttribute("role", "option");
					option.dataset.index = String(index);
					option.innerHTML =
						'<span class="fb-autocomplete-label">' +
						escapeHtml(item.label || "") +
						"</span>" +
						(item.subtitle
							? '<span class="fb-autocomplete-subtitle">' + escapeHtml(item.subtitle) + "</span>"
							: "");

					option.addEventListener("mousedown", function (event) {
						event.preventDefault();
						applySelection(index);
					});

					list.appendChild(option);
				});

				control.classList.add("is-open");
				textInput.setAttribute("aria-expanded", "true");
			}

			function applySelection(index) {
				var item = items[index];
				if (!item) {
					return;
				}

				hiddenInput.value = item.id;
				textInput.value = item.label;
				textInput.setAttribute("data-selected-label", item.label);
				closeList();

				if (status) {
					status.textContent = "Odabrano: " + item.label;
				}

				if (window.jQuery && window.jQuery.validator) {
					window.jQuery(hiddenInput).valid();
				}
			}

			function updateHighlight() {
				var options = list.querySelectorAll(".fb-autocomplete-item");
				options.forEach(function (option, index) {
					option.classList.toggle("is-active", index === selectedIndex);
				});
			}

			var fetchItems = debounce(function () {
				var term = textInput.value.trim();

				if (activeController) {
					activeController.abort();
				}

				activeController = new AbortController();

				var query = new URLSearchParams();
				query.set("term", term);

				fetch(endpoint + "?" + query.toString(), {
					headers: {
						"X-Requested-With": "XMLHttpRequest"
					},
					signal: activeController.signal
				})
					.then(function (response) {
						if (!response.ok) {
							throw new Error("Autocomplete failed");
						}

						return response.json();
					})
					.then(function (data) {
						items = Array.isArray(data) ? data : [];
						selectedIndex = -1;
						render();
					})
					.catch(function (error) {
						if (error.name === "AbortError") {
							return;
						}

						items = [];
						render();
					});
			}, 200);

			textInput.addEventListener("focus", function () {
				fetchItems();
			});

			textInput.addEventListener("input", function () {
				if (hiddenInput.value) {
					hiddenInput.value = "";
				}
				fetchItems();
			});

			textInput.addEventListener("keydown", function (event) {
				if (!control.classList.contains("is-open") && ["ArrowDown", "ArrowUp"].includes(event.key)) {
					fetchItems();
					return;
				}

				if (event.key === "ArrowDown") {
					event.preventDefault();
					selectedIndex = Math.min(selectedIndex + 1, items.length - 1);
					updateHighlight();
				}

				if (event.key === "ArrowUp") {
					event.preventDefault();
					selectedIndex = Math.max(selectedIndex - 1, 0);
					updateHighlight();
				}

				if (event.key === "Enter" && selectedIndex >= 0) {
					event.preventDefault();
					applySelection(selectedIndex);
				}

				if (event.key === "Escape") {
					closeList();
				}
			});

			textInput.addEventListener("blur", function () {
				setTimeout(function () {
					if (!hiddenInput.value) {
						var fallback = textInput.getAttribute("data-selected-label") || "";
						textInput.value = fallback;
						if (!fallback) {
							textInput.placeholder = placeholder;
						}
					}

					closeList();
					if (window.jQuery && window.jQuery.validator) {
						window.jQuery(hiddenInput).valid();
					}
				}, 120);
			});

			document.addEventListener("click", function (event) {
				if (!control.contains(event.target)) {
					closeList();
				}
			});
		});
	}

	function initDateTimeInputs() {
		var controls = document.querySelectorAll("[data-datetime-control]");
		var hasFlatpickr = typeof window.flatpickr === "function";

		controls.forEach(function (control) {
			var hidden = control.querySelector("[data-datetime-value]");
			var text = control.querySelector("[data-datetime-display]");
			var locale = (navigator.language || "en-US").toLowerCase();

			if (!hidden || !text) {
				return;
			}

			function syncDisplayFromHidden() {
				var source = hidden.value;
				if (!source) {
					text.value = "";
					return;
				}

				var parsed = new Date(source);
				if (!Number.isNaN(parsed.getTime())) {
					text.value = formatLocaleDateTime(parsed, locale);
				}
			}

			if (hasFlatpickr) {
				text.readOnly = true;
				var initialDate = hidden.value ? new Date(hidden.value) : null;
				var hasInitialDate = initialDate && !Number.isNaN(initialDate.getTime());

				window.flatpickr(text, {
					enableTime: true,
					time_24hr: true,
					allowInput: false,
					dateFormat: "d.m.Y H:i",
					defaultDate: hasInitialDate ? initialDate : null,
					onChange: function (selectedDates) {
						var selectedDate = selectedDates[0];
						if (!selectedDate || Number.isNaN(selectedDate.getTime())) {
							hidden.value = "";
							control.classList.add("has-error");
							return;
						}

						control.classList.remove("has-error");
						hidden.value = toLocalIsoString(selectedDate);

						if (window.jQuery && window.jQuery.validator) {
							window.jQuery(hidden).valid();
						}
					}
				});

				if (!hasInitialDate) {
					text.value = "";
				}
				return;
			}

			syncDisplayFromHidden();

			text.addEventListener("blur", function () {
				var parsed = parseLocaleDateTime(text.value, locale);

				if (!parsed) {
					control.classList.add("has-error");
					return;
				}

				control.classList.remove("has-error");
				hidden.value = toLocalIsoString(parsed);
				text.value = formatLocaleDateTime(parsed, locale);

				if (window.jQuery && window.jQuery.validator) {
					window.jQuery(hidden).valid();
				}
			});
		});
	}

	function initNavbarCollapseEffects() {
		var navbarCollapse = document.getElementById("mainNav");
		if (!navbarCollapse) {
			return;
		}

		var navbar = navbarCollapse.closest(".fb-navbar");
		if (!navbar) {
			return;
		}

		navbarCollapse.addEventListener("show.bs.collapse", function () {
			navbar.classList.remove("is-closing");
			navbar.classList.add("is-opening");
		});

		navbarCollapse.addEventListener("shown.bs.collapse", function () {
			navbar.classList.remove("is-opening");
			navbar.classList.add("is-open");
		});

		navbarCollapse.addEventListener("hide.bs.collapse", function () {
			navbar.classList.remove("is-open", "is-opening");
			navbar.classList.add("is-closing");
		});

		navbarCollapse.addEventListener("hidden.bs.collapse", function () {
			navbar.classList.remove("is-closing");
		});
	}

	function initValidationOnBlur() {
		if (!window.jQuery || !window.jQuery.validator) {
			return;
		}

		window.jQuery("form").each(function () {
			var form = this;
			if (!window.jQuery(form).data("validator")) {
				return;
			}

			window.jQuery(form)
				.find("input, select, textarea")
				.not("[type='hidden'], [type='button'], [type='submit'], [type='reset']")
				.on("blur change", function () {
					window.jQuery(this).valid();
				});
		});
	}

	function initCrudDropdownAnimations() {
		var dropdowns = document.querySelectorAll(".fb-crud-dropdown");

		dropdowns.forEach(function (dropdown) {
			var menu = dropdown.querySelector(".fb-crud-menu");
			var cleanupTimeoutId = 0;

			if (!menu) {
				return;
			}

			function clearClosingState() {
				window.clearTimeout(cleanupTimeoutId);
				menu.classList.remove("is-closing");
			}

			dropdown.addEventListener("show.bs.dropdown", function () {
				clearClosingState();
			});

			dropdown.addEventListener("hide.bs.dropdown", function () {
				if (prefersReducedMotion()) {
					return;
				}

				menu.classList.add("is-closing");
			});

			dropdown.addEventListener("hidden.bs.dropdown", function () {
				if (prefersReducedMotion()) {
					clearClosingState();
					return;
				}

				cleanupTimeoutId = window.setTimeout(clearClosingState, 480);
			});
		});
	}

	function initCatchImageWaveEffect() {
		var frames = document.querySelectorAll(".fb-catch-image-frame");

		frames.forEach(function (frame) {
			if (frame.dataset.fbWaveInit === "true") {
				return;
			}

			frame.dataset.fbWaveInit = "true";

			var rafId = 0;
			var lastTimestamp = 0;
			var hoverActive = false;
			var focusActive = false;
			var waveTopX = 0;
			var waveBottomX = 0;
			var waveLeftY = 0;
			var waveRightY = 0;
			var speedPxPerSecond = 44;

			function applyWaveVars() {
				frame.style.setProperty("--fb-wave-top-x", waveTopX.toFixed(2) + "px");
				frame.style.setProperty("--fb-wave-bottom-x", waveBottomX.toFixed(2) + "px");
				frame.style.setProperty("--fb-wave-left-y", waveLeftY.toFixed(2) + "px");
				frame.style.setProperty("--fb-wave-right-y", waveRightY.toFixed(2) + "px");
			}

			function step(timestamp) {
				if (!(hoverActive || focusActive)) {
					rafId = 0;
					lastTimestamp = 0;
					return;
				}

				if (!lastTimestamp) {
					lastTimestamp = timestamp;
				}

				var deltaSeconds = (timestamp - lastTimestamp) / 1000;
				lastTimestamp = timestamp;
				var delta = speedPxPerSecond * deltaSeconds;

				waveTopX += delta;
				waveRightY += delta;
				waveBottomX -= delta;
				waveLeftY -= delta;
				applyWaveVars();

				rafId = window.requestAnimationFrame(step);
			}

			function startLoop() {
				frame.classList.add("is-wave-active");
				if (rafId) {
					return;
				}
				rafId = window.requestAnimationFrame(step);
			}

			function stopLoop() {
				frame.classList.remove("is-wave-active");
				if (rafId) {
					window.cancelAnimationFrame(rafId);
					rafId = 0;
				}
				lastTimestamp = 0;
			}

			function syncLoopState() {
				if (hoverActive || focusActive) {
					startLoop();
					return;
				}

				stopLoop();
			}

			function handleMouseEnter() {
				hoverActive = true;
				syncLoopState();
			}

			function handleMouseLeave() {
				hoverActive = false;
				syncLoopState();
			}

			function handleFocusIn() {
				focusActive = true;
				syncLoopState();
			}

			function handleFocusOut(event) {
				if (event.relatedTarget && frame.contains(event.relatedTarget)) {
					return;
				}

				focusActive = false;
				syncLoopState();
			}

			function cleanup() {
				hoverActive = false;
				focusActive = false;
				stopLoop();
			}

			frame.addEventListener("mouseenter", handleMouseEnter);
			frame.addEventListener("mouseleave", handleMouseLeave);
			frame.addEventListener("focusin", handleFocusIn);
			frame.addEventListener("focusout", handleFocusOut);
			window.addEventListener("pagehide", cleanup, { once: true });
		});
	}

	document.addEventListener("DOMContentLoaded", function () {
		initPageLoadReveal();
		initAjaxSearchTables();
		initAutocompleteDropdowns();
		initDateTimeInputs();
		initNavbarCollapseEffects();
		initValidationOnBlur();
		initCrudDropdownAnimations();
		initCatchImageWaveEffect();
	});
})();
