using System;
using System.Collections.Generic;
[Serializable]
public struct Geometry
{
    public float width;
    public float height;
}
[Serializable]
public struct VNCoptions
{
    public string address;
    public string password;
}
[Serializable]
public struct Screen
{
    public string ID;
    public float posX;
    public float posY;
    
   // private List<string> neighbours;
   public VNCoptions options;

    public Screen(string _ID,float _posX, float _posY, List<string> _neighbours, VNCoptions _options)
    {
        this.ID = _ID;
        this.posX = _posX;
        this.posY = _posY;
      //  this.neighbours = _neighbours;
        this.options = _options;
    }

    public VNCoptions GetVNCoptions()
    {
        return options;
    }
}
[Serializable]
public class Wall
{
    public string name;
    public float min;
    public float max;
    public bool interaction;
    public List<Screen> screens;
    public float width;
    public float height;
    public float scalex;
    public float scaley;
    public float overlap;
    
    public Wall(string _name, float _min, float _max,float _width,float _height, List<Screen> _screens)
    {
        this.name = _name;
        this.min = _min;
        this.max = _max;
        this.screens = _screens;
        this.width = _width;
        this.height = _height;
    }
    
    public string GetName(){ return this.name;}
    public float GetMin(){ return this.min;}
    public float GetMax(){ return this.max;}

    public List<Screen> GetScreens(){ return this.screens;}

}

[Serializable]
public class VWsession 
{
    public Geometry geom;
    public List<Wall> lstWall;

    public VWsession(Geometry _geom, List<Wall> _lstWall)
    {
        this.geom = _geom;
        this.lstWall = _lstWall;
    }

    public Geometry GetGeom(){
        return geom;
    }


    public List<Wall> GetLstWall(){
        return lstWall;
    }

}
