// GPLib
// Copyright (c) 2007 - 2008 Jeff Myers
//
// This package is free software;  you can redistribute it and/or
// modify it under the terms of the license found in the file
// named COPYING that should have accompanied this file.
//
// THIS PACKAGE IS PROVIDED ``AS IS'' AND WITHOUT ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, WITHOUT LIMITATION, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
//

#include "gpOpenGL.h"

#include <math.h>
#define GL_INVALID_ID 0xffffffff

void glSetColor ( float c[3], float alpha )
{
	glColor4f(c[0],c[1],c[2],alpha);
}

void glTranslatefv ( float v[3] )
{
	glTranslatef(v[0],v[1],v[2]);
}

void glQuad ( float x, float y, eAlignment align, float scale )
{
	glPushMatrix();

	x *= scale;
	y *= scale;

	switch ( align )
	{
	case eCenter:
		glTranslatef(-x*0.5f,-y*0.5f,0);
		break;
	case eLowerLeft:
		break;
	case eLowerRight:
		glTranslatef(-x,0,0);
		break;
	case eUpperLeft:
		glTranslatef(0,-y,0);
		break;
	case eUpperRight:
		glTranslatef(-x,-y,0);
		break;
	case eCenterLeft:
		glTranslatef(0,-y*0.5f,0);
		break;
	case eCenterRight:
		glTranslatef(-x,-y*0.5f,0);
		break;
	case eCenterTop:
		glTranslatef(-x*0.5f,-y,0);
		break;
	case eCenterBottom:
		glTranslatef(-x*0.5f,0,0);
		break;
	}

	glBegin(GL_QUADS);

	glNormal3f(0,0,1);
	glTexCoord2f(0,1);
	glVertex3f(0,0,0);

	glTexCoord2f(1,1);
	glVertex3f(x,0,0);

	glTexCoord2f(1,0);
	glVertex3f(x,y,0);

	glTexCoord2f(0,0);
	glVertex3f(0,y,0);

	glEnd();

	glPopMatrix();
}

#define GL_RAD_CON 0.017453292519943295769236907684886f

void glLineRing ( float radius, float width )
{
	int segments = 180;

	glDisable(GL_TEXTURE_2D);
	glLineWidth(width);
	glBegin(GL_LINE_LOOP);
	for (float a = 0; a < 360.0f; a += segments/360.0f)
		glVertex3f(cosf(a*GL_RAD_CON)*radius,sinf(a*GL_RAD_CON)*radius,0);
	glEnd();
	glLineWidth(1);
	glEnable(GL_TEXTURE_2D);
}

//--------------------GLColor----------------------

GLColor::GLColor( const float *v )
{
	memcpy(color,v,sizeof(float)*4);
}

GLColor::GLColor( float c[3], float a )
{
	memcpy(color,c,sizeof(float)*3);
	color[3] = a;	
}

GLColor::GLColor( float r, float g, float b, float a )
{
	color[0] = r;
	color[1] = g;
	color[2] = b;
	color[3] = a;
}

void GLColor::setGL ( void )
{
	glColor4fv(color);
}

GLColor& GLColor::operator = ( const GLColor & c)
{
	memcpy(color,c.color,sizeof(float)*4);
	return *this;
}

bool GLColor::operator == ( const GLColor &c ) const
{
	for (int i = 0; i < 4; i++)
	{
		if ( color[i] != c.color[i] )
			return false;
	}
	return true;
}

void GLColor::set( const float *v )
{
	memcpy(color,v,sizeof(float)*4);
}

void GLColor::set( float r, float g, float b , float a )
{
  color[0] = r;
  color[1] = g;
  color[2] = b;
  color[3] = a;
}

// Local Variables: ***
// mode:C++ ***
// tab-width: 8 ***
// c-basic-offset: 2 ***
// indent-tabs-mode: t ***
// End: ***
// ex: shiftwidth=2 tabstop=8
