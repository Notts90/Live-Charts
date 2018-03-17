﻿#region License
// The MIT License (MIT)
// 
// Copyright (c) 2016 Alberto Rodríguez Orozco & LiveCharts contributors
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights to 
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
// of the Software, and to permit persons to whom the Software is furnished to 
// do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#region

using System.Drawing;
using LiveCharts.Core.Abstractions;
using LiveCharts.Core.Abstractions.DataSeries;
using LiveCharts.Core.Charts;
using LiveCharts.Core.Coordinates;
using LiveCharts.Core.Data;
using LiveCharts.Core.Drawing;
using LiveCharts.Core.Drawing.Svg;
using LiveCharts.Core.Interaction;
using LiveCharts.Core.ViewModels;

#endregion

namespace LiveCharts.Core.DataSeries
{
    /// <summary>
    /// The scatter series class.
    /// </summary>
    public class ScatterSeries<TModel> 
        : CartesianSeries<TModel, WeightedPoint, ScatterViewModel, Point<TModel, WeightedPoint, ScatterViewModel>>, IScatterSeries
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScatterSeries{TModel}"/> class.
        /// </summary>
        public ScatterSeries()
        {
            MaxGeometrySize = 30f;
            MinGeometrySize = 14f;
            StrokeThickness = 1f;
            Geometry = Geometry.Circle;
            Charting.BuildFromSettings<IScatterSeries>(this);
            RangeByDimension = RangeByDimension = new[]
            {
                new RangeF(), // x
                new RangeF(), // y
                new RangeF()  // w
            };
        }

        /// <inheritdoc />
        public Geometry PointGeometry { get; set; }

        /// <inheritdoc />
        public float MaxGeometrySize { get; set; }

        /// <inheritdoc />
        public float MinGeometrySize { get; set; }

        /// <inheritdoc />
        public override float[] DefaultPointWidth => new []{0f,0f};

        /// <inheritdoc />
        public override float[] PointMargin => new[] {MaxGeometrySize, MaxGeometrySize};

        /// <inheritdoc />
        public override void UpdateView(ChartModel chart)
        {
            var cartesianChart = (CartesianChartModel)chart;
            var x = cartesianChart.Dimensions[0][ScalesAt[0]];
            var y = cartesianChart.Dimensions[1][ScalesAt[1]];

            var uw = chart.Get2DUiUnitWidth(x, y);

            var p1 = new PointF(RangeByDimension[2].Min, MinGeometrySize);
            var p2 = new PointF(RangeByDimension[2].Max, MaxGeometrySize);

            int xi = 0, yi = 1;
            if (chart.InvertXy)
            {
                xi = 1;
                yi = 0;
            }

            Point<TModel, WeightedPoint, ScatterViewModel> previous = null;
            foreach (var current in Points)
            {
                var p = new[]
                {
                    chart.ScaleToUi(current.Coordinate[0][0], x),
                    chart.ScaleToUi(current.Coordinate[1][0], y),
                    cartesianChart.LinealScale(p1, p2, current.Coordinate.Weight)
                };

                var vm = new ScatterViewModel
                {
                    Location = Perform.Sum(new PointF(p[xi], p[yi]), new PointF(uw[0], uw[1])),
                    Diameter = p[2]
                };

                if (current.View == null)
                {
                    current.View = PointViewProvider();
                }

                current.ViewModel = vm;
                current.View.DrawShape(current, previous);

                current.InteractionArea = new RectangleInteractionArea(
                    new RectangleF(
                        vm.Location.X - p[2] * .5f,
                        vm.Location.Y - p[2] * .5f,
                        p[2],
                        p[2]));
                previous = current;
            }
        }

        /// <inheritdoc />
        protected override IPointView<TModel, Point<TModel, WeightedPoint, ScatterViewModel>, WeightedPoint, ScatterViewModel> 
            DefaultPointViewProvider()
        {
            return Charting.Current.UiProvider.GetNewScatterView<TModel>();
        }
    }
}