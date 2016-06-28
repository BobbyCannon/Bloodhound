'use strict';

var site = site || angular.module('Site', []);

site.directive('ngWidget', function () {
	
	function createChart(chartContext, model) {
		switch (model.chartType) {
			case 'bar':
				return new Chart(chartContext).Bar(model.data, { });

			case 'line':
				return new Chart(chartContext).Line(model.data, { bezierCurve: false, pointDot: false });

			case 'pie':
				return new Chart(chartContext).Pie(model.data.datasets, { animateScale: false });

			case 'donut':
				return new Chart(chartContext).Doughnut(model.data.datasets, { animateScale: false });

			default:
				return undefined;
		}
	}

	function setElementWidth($element, size) {
		switch (size) {
			case 'large':
				$element.width(1260);
				break;

			case 'medium':
				$element.width(620);
				break;

			case 'small':
			default :
				$element.width(300);
				break;
		}
	}

	return {
		restrict: 'A',
		scope: {
			model: '=ngModel',
			remove: '=ngRemove',
			previous: '=ngPrevious',
			next: '=ngNext'
		},
		link: function (scope, $element) {
			var $chart = undefined;
			var $canvas = $('<canvas />');
			var $header = $('<div />');
			var $controls = $('<div />');
			var $label = $('<label />');
			var $table = $('<table />');
			var $removeButton = $('<i class="fa fa-remove pull-right" />');
			
			$removeButton.click(function () {
				scope.remove(scope.model);
				scope.$apply();
			});

			$element.addClass('widget');
			$controls.append($removeButton);

			if (scope.next !== undefined) {
				var $nextButton = $('<i class="fa fa-chevron-right pull-right" />');
				$nextButton.click(function () { scope.next(scope.model, 'down'); });
				$controls.append($nextButton);
			}

			if (scope.previous !== undefined) {
				var $previousButton = $('<i class="fa fa-chevron-left pull-right" />');
				$previousButton.click(function () { scope.next(scope.model, 'up'); });
				$controls.append($previousButton);
			}

			$header.append($controls);
			$header.append($label);
			$element.append($header);
			$element.append($canvas);
			$element.append($table);

			function drawChart() {
				if ($chart !== undefined) {
					$chart.destroy();
				}

				if (scope.model === undefined || scope.model.chartSize === undefined) {
					return;
				}

				$element.show();
				$label.text(scope.model.name);

				if (scope.model.chartType === 'list') {
					$table.empty();
					$table.show();
					$canvas.hide();

					setElementWidth($table, scope.model.chartSize);
					
					for (var index in scope.model.data.datasets) {
						if (scope.model.data.datasets.hasOwnProperty(index)) {
							var item = scope.model.data.datasets[index];
							$table.append('<tr><td class=\'label\'>' + item.label + '</td><td class=\'value\'>' + item.value + ' ' + scope.model.aggregateByFormat + '</td></tr>');
						}
					}
				} else {
					$table.hide();
					$canvas.show();
					setElementWidth($canvas, scope.model.chartSize);
					$canvas.height(230);
					$chart = createChart($canvas[0].getContext('2d'), scope.model);
				}
			}

			scope.$watch('model', drawChart);
			drawChart();
		}
	};
});

Array.prototype.move = function (oldIndex, newIndex) {
	if (newIndex >= this.length) {
		var k = newIndex - this.length;
		while ((k--) + 1) {
			this.push(undefined);
		}
	}
	this.splice(newIndex, 0, this.splice(oldIndex, 1)[0]);
	return this; // for testing purposes
};