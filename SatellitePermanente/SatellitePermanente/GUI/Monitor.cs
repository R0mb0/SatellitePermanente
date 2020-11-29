﻿using SatellitePermanente.Database;
using SatellitePermanente.GUI;
using SatellitePermanente.GUI.GrayMapUtility;
using SatellitePermanente.LogicAndMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SatellitePermanente
{


    public partial class Home : Form
    {
        /*Fields*/
        private AddPoint addPoint;
        private DeletePoint deletePoint;
        private Debug debug;
        private bool status = false; /*This is the status of the current database is it exsit or not*/
        private DatabaseWithRescue database; 
       
        /*Builder*/
        public Home()
        {
            InitializeComponent();
            addPoint = new AddPoint();
            database = new DatabaseWithRescue();
        }

        /*In this method is launched the "AddPoint gui", and is the part of code where the point created in AddPoint gui is added to database*/
        private void AddPoint_Click(object sender, EventArgs e)
        {
            FormBridge.returnPoint = null;
           
            addPoint.ShowDialog();

            /*In case of the gui is closed without any action from the user*/
            if (FormBridge.returnPoint == null)
            {
                return;
            }

            this.status = true;

            /*Verify the corretly add of the new point*/
            try
            {
                this.database.AddPoint(FormBridge.returnPoint);
                this.GrayMap.Refresh();
            }
            catch (Exception error) 
            {
                MessageBox.Show("DATABASE ERROR!\n"+"Error message:" + error.Message);
                return;
            }
        }

        /*This method launch the "DeletePoint" gui and eliminate the returned point*/
        private void DeletePoint_Click(object sender, EventArgs e)
        {
            if (!status)
            {
                MessageBox.Show("The database dosn`t exsist, you need to load it!");
                return;
            }
            FormBridge.returnDatabase = this.database;
            FormBridge.returnInteger = null; /*This is the index of the point to delete*/
            deletePoint = new DeletePoint();
            deletePoint.ShowDialog();

            /*In case of the gui is closed without any action from the user*/
            if (FormBridge.returnInteger == null)
            {
                return;
            }
            
            /*Try to delete point from index*/
            try
            {

                if (this.database.DeletePointFromIndex(Convert.ToInt32(FormBridge.returnInteger)))
                {
                    MessageBox.Show("Point removed successfully!");
                    this.GrayMap.Refresh();
                }
                else
                {
                    MessageBox.Show("Error while removing!");
                }
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.Message);
            }
        }

        /*This method invokes the procedure for save the current database*/
        private void Save_Click(object sender, EventArgs e)
        {
            if (this.database.SaveDatabase())
            {
                MessageBox.Show("Successfully saved!");
            }
            else
            {
                MessageBox.Show("Error while saving!");
            }
        }

        /*this method invokes the procedure for load the last database salved*/
        private void Load_Click(object sender, EventArgs e)
        {
            if (this.database.LoadDatabase())
            {
                this.status = true;
                MessageBox.Show("Successfully loaded!");

                this.GrayMap.Refresh();
            }
            else
            {
                MessageBox.Show("Error while loading!");
            }
        }

        /*This is a method of debug; where is possible to look the state of the current database*/
        private void Debug_Click(object sender, EventArgs e)
        {
            /*If doesn`t exsist a current database in memory, the user must be unable to acces the debug page*/
            if (this.status)
            {
                FormBridge.returnDatabase = this.database;
                debug = new Debug();
                debug.ShowDialog();
            }
            else
            {
                MessageBox.Show("The database dosn`t exsist, you need to load it!");
            }
        }

        /*----------------------------------------------------------GREYMAP INITIALIZE--------------------------------------------*/
        private void GreyMap_Paint(object sender, PaintEventArgs e)
        {
            /*Local Fields*/
            Graphics dc = e.Graphics;

                int indx = 0;
                Pen pen = Pens.Gray;
                List<System.Drawing.Point>? pointList = LatitudeLongitudePoints.GetAxes(456, 942);

                while (indx != pointList.Count)/*this ia a inizialize operation, must be did one time*/
                {
                    dc.DrawLine(pen, pointList[indx], pointList[indx + 1]);
                    indx = indx + 2;
                }
               
            

            if(database.pointList.Count == 0)
            {
                return;
            }
            
            
            MaxCoordinates maxCoordinates = new MaxCoordinates();
            maxCoordinates.maxLatitude = database.maxLatitude;
            maxCoordinates.minLatitude = database.minLatitude;
            maxCoordinates.maxLongitude = database.maxLongitude;
            maxCoordinates.minLongitude = database.minLongitude;
            

            ConvertToGraphic graphic = new ConvertToGraphic(456, 942, maxCoordinates);

            GrayMapValues.coordinates = graphic.extremeCoordinates;

            WriteAxesValue(graphic);


            this.database.pointList.ForEach(delegate (LogicAndMath.Point myPoint) 
            {

                if (myPoint.meetingPoint)
                {
                    System.Drawing.Point stringPoint = new System.Drawing.Point();
                    stringPoint.X = graphic.GetDrawingPoint(myPoint).X + 10;
                    stringPoint.Y = graphic.GetDrawingPoint(myPoint).Y + 10;
                    dc.DrawEllipse(Pens.DarkGreen, graphic.GetDrawingPoint(myPoint).X -7, graphic.GetDrawingPoint(myPoint).Y -7, 14, 14);
                    dc.FillEllipse(Brushes.DarkGreen, graphic.GetDrawingPoint(myPoint).X -7, graphic.GetDrawingPoint(myPoint).Y-7, 14, 14);
                    dc.DrawString(myPoint.GetPointString(), new Font(this.Font.FontFamily, this.Font.Size - 2), Brushes.Black, stringPoint);
                }
                else
                {
                    System.Drawing.Point stringPoint = new System.Drawing.Point();
                    stringPoint.X = graphic.GetDrawingPoint(myPoint).X + 10;
                    stringPoint.Y = graphic.GetDrawingPoint(myPoint).Y + 10;

                    dc.DrawEllipse(Pens.DarkRed, graphic.GetDrawingPoint(myPoint).X -7, graphic.GetDrawingPoint(myPoint).Y -7, 14, 14);
                    dc.FillEllipse(Brushes.DarkRed, graphic.GetDrawingPoint(myPoint).X -7, graphic.GetDrawingPoint(myPoint).Y-7, 14, 14);

                    dc.DrawString(myPoint.GetPointString(), new Font(this.Font.FontFamily, this.Font.Size -2), Brushes.Black, stringPoint);
                    
                }
            });

            this.database.nodeList.ForEach(delegate (Node myNode) 
            {
                System.Drawing.Point medPoint = new System.Drawing.Point();
                medPoint.X = (graphic.GetDrawingPoint(myNode.pointB).X  + graphic.GetDrawingPoint(myNode.pointA).X) /2;
                medPoint.Y = (graphic.GetDrawingPoint(myNode.pointB).Y  + graphic.GetDrawingPoint(myNode.pointA).Y) /2;

                dc.DrawLine(Pens.Black, graphic.GetDrawingPoint(myNode.pointA), graphic.GetDrawingPoint(myNode.pointB));
                dc.DrawString(myNode.GetDistanceString(), new Font(this.Font.FontFamily, this.Font.Size - 2), Brushes.Black, medPoint);


            });
            
        }

        private void WriteAxesValue(ConvertToGraphic extremes)
        {
            decimal latitudeUnit = (extremes.DLat /3);
            decimal longitudeUnit = (extremes.DLon /3);


            decimal latitude = extremes.extremeCoordinates.minLatitude.GetLatitude();
            decimal longitude = extremes.extremeCoordinates.minLongitude.GetLongitude();


            /*Operations*/
            latitude = latitude + latitudeUnit;
            Origin coordinate = Utility.ConvertToSexagesimal(latitude);
            if (latitude > 0)
            {
                this.TextBoxLatutude1.Text = "N " + coordinate.degrees +"° " + coordinate.prime +"' " + Math.Round(coordinate.latter, 3) + "''";
            }
            else
            {
                this.TextBoxLatutude1.Text = "S " + coordinate.degrees + "° " + coordinate.prime + "' " + Math.Round(coordinate.latter, 3) + "''";
            }

            latitude = latitude + latitudeUnit;
            coordinate = Utility.ConvertToSexagesimal(latitude);
            if (latitude > 0)
            {
                this.TextBoxLatitude2.Text = "N " + coordinate.degrees + "° " + coordinate.prime + "' " + Math.Round(coordinate.latter, 3) + "''";
            }
            else
            {
                this.TextBoxLatitude2.Text = "S " + coordinate.degrees + "° " + coordinate.prime + "' " + Math.Round(coordinate.latter, 3) + "''";
            }

            longitude = longitude + longitudeUnit;
            coordinate = Utility.ConvertToSexagesimal(longitude);
            if(longitude > 0)
            {
                this.TextBoxLongitude1.Text = "E " + coordinate.degrees + "° " + coordinate.prime + "' " + Math.Round(coordinate.latter, 3) + "''";
            }
            else
            {
                this.TextBoxLongitude1.Text = "W " + coordinate.degrees + "° " + coordinate.prime + "' " + Math.Round(coordinate.latter, 3) + "''";
            }

            longitude = longitude + longitudeUnit;
            coordinate = Utility.ConvertToSexagesimal(longitude);
            if (longitude > 0)
            {
                this.TextBoxLongitude2.Text = "E " + coordinate.degrees + "° " + coordinate.prime + "' " + Math.Round(coordinate.latter, 3) + "''";
            }
            else
            {
                this.TextBoxLongitude2.Text = "W " + coordinate.degrees + "° " + coordinate.prime + "' " + Math.Round(coordinate.latter, 3) + "''";
            }


        }

    }
}
