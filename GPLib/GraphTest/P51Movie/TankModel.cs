using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using Drawables.Textures;
using Drawables.StaticModels;
using OpenTK;
using OpenTK.Graphics.OpenGL;

using Math3D;

namespace P51Movie
{
    public class PathDrivenTank
    {
        public Tank TankSprite;
        public Texture Shot;

        public Texture JumpJet, ShotGlow, JetGlow;

        public Vector3 InitalPos = Vector3.Zero;
        public double InitalRot = 0;

        public class AnimationEffectInstance
        {
            public Vector3 Position = Vector3.Zero;
            public Vector3 Vector = Vector3.Zero;
            public double Lifetime = 0;
        }

        public List<AnimationEffectInstance> Shots = new List<AnimationEffectInstance>();

        public class PathSegment
        {
            public enum SegmentType
            {
                NoMove,
                Drive,
                Shoot,
                Jump,
                WaitTillLand,
                Delay,
                SetTurn,
                SetPos,
            }

            public SegmentType Segment = SegmentType.NoMove;
            public Vector2 Input = Vector2.Zero;
            public double LifeTime = 0;

            public PathSegment()
            {}

            public PathSegment ( SegmentType segmentType, Vector2 input, double life )
            {
                Segment = segmentType;
                Input = input;
                LifeTime = life;
            }
        }

        public bool Loop = true;

        public double JumpJetSize = 0;
        public double JumpDecay = 1.0;
        public double InitalJumpFlameSize = 2.0;

        public double JumpImpulse = 25;
        public double ShotSpeed = 40;
        public double ShotLife = 8;
        public double ShotGlowRadius = 3;

        public Vector2 BarrelOffset = new Vector2(4.5f, 1.6f);

        int SegmentIndex = -1;
        double SegmentStartTime = 0;

        public List<PathSegment> Segments = new List<PathSegment>();

        public PathDrivenTank ( Tank tank, Texture shot )
        {
            TankSprite = tank;
            Shot = shot;

            JumpJet = TextureSystem.system.GetTexture(Sceene.API.GetFile("img/jumpjets.png"));
            ShotGlow = TextureSystem.system.GetTexture(Sceene.API.GetFile("img/sphere_glow.png"));
            JetGlow = TextureSystem.system.GetTexture(Sceene.API.GetFile("img/sphere_glow.png"));
        }

        public void Update ( double time, double delta )
        {
            bool jumped = false;

            if (Segments.Count > 0 && SegmentIndex < Segments.Count)
            {
                if (SegmentIndex == -1)
                {
                    SegmentStartTime = time;
                    SegmentIndex = 0;
                    TankSprite.Position = InitalPos;
                    TankSprite.Rotation = InitalRot;
                }

                PathSegment seg = Segments[SegmentIndex];
                
                switch (seg.Segment)
                {
                    case PathSegment.SegmentType.NoMove:
                        TankSprite.InputVectors = Vector2.Zero;
                        break;

                    case PathSegment.SegmentType.Drive:
                        TankSprite.InputVectors = seg.Input;
                        break;

                    case PathSegment.SegmentType.SetTurn:
                        TankSprite.InputVectors = Vector2.Zero;
                        TankSprite.Rotation = seg.Input.Y;
                        break;

                    case PathSegment.SegmentType.SetPos:
                        TankSprite.InputVectors = Vector2.Zero;
                        TankSprite.Position.X = seg.Input.X;
                        TankSprite.Position.Y = seg.Input.Y;
                        break;

                    case PathSegment.SegmentType.Jump:
                        jumped = true;
                        TankSprite.JumpVel = JumpImpulse;
                        if (seg.Input.Y != 0)
                            TankSprite.RotVel = TankSprite.Speeds.Y * seg.Input.Y;
                        break;

                    case PathSegment.SegmentType.Shoot:
                        {
                            AnimationEffectInstance shot = new AnimationEffectInstance();
                            Vector2 heading = VectorHelper2.FromAngle(TankSprite.Rotation);
                            shot.Position = new Vector3(TankSprite.Position.X + heading.X * BarrelOffset.X, TankSprite.Position.Y + heading.Y * BarrelOffset.X, TankSprite.Position.Z + BarrelOffset.Y);
                            shot.Vector = new Vector3(heading.X * (float)ShotSpeed, heading.Y * (float)ShotSpeed, 0);
                            shot.Lifetime = ShotLife;
                            Shots.Add(shot);
                        }
                        break;
                }
                double age = time - SegmentStartTime;
                if ( age >= seg.LifeTime )
                {
                    if (seg.Segment != PathSegment.SegmentType.WaitTillLand || ((TankSprite.Position.Z <= 0) && (TankSprite.JumpVel <= 0)))
                    {
                        if (seg.Segment != PathSegment.SegmentType.WaitTillLand)
                            SegmentStartTime = SegmentStartTime + seg.LifeTime;
                        else
                            SegmentStartTime = time;

                        SegmentIndex++;
                        if (SegmentIndex >= Segments.Count && Loop)
                        {
                            SegmentIndex = 0;
                            SegmentStartTime = time;
                        }
                    }
                }
            }
            else
                TankSprite.InputVectors = Vector2.Zero;

            if (jumped)
                JumpJetSize = InitalJumpFlameSize;
            else if (JumpJetSize > 0)
            {
                JumpJetSize -= JumpDecay * delta;
                if (JumpJetSize < 0)
                    JumpJetSize = 0;
            }

            TankSprite.Update(time, delta);

            List<AnimationEffectInstance> deadShots = new List<AnimationEffectInstance>();

            foreach (AnimationEffectInstance shot in Shots)
            {
                // move the shots
                shot.Position += shot.Vector * (float)delta;
                shot.Lifetime -= delta;
                if (shot.Lifetime <= 0)
                    deadShots.Add(shot);
            }

            foreach (AnimationEffectInstance shot in deadShots)
                Shots.Remove(shot);
        }

        public void Draw ( double time )
        {
            TankSprite.Draw(time);
        }

        public void DrawTrans ( double time )
        {
            TankSprite.DrawTrans(time);

            GL.Disable(EnableCap.Lighting);
            GL.DepthMask(false);
            GL.Disable(EnableCap.DepthTest);

            foreach (AnimationEffectInstance shot in Shots)
            {
                if (shot.Position.Z < ShotGlowRadius)
                {
                    GL.Color4(1, 1, 1, 0.5);
                    GL.PushMatrix();
                    GL.Translate(shot.Position.X, shot.Position.Y, 0.001);
                    float factor = 1.0f / (shot.Position.Z / (float)ShotGlowRadius);
                    ShotGlow.DrawCentered((float)ShotGlowRadius * factor, (float)ShotGlowRadius * factor);
                    GL.PopMatrix();
                    GL.Color4(1, 1, 1, 1.0);
                }
            }

            GL.Enable(EnableCap.DepthTest);

            if (JumpJetSize > 0)
            {
                float maxHeight = 1.5f;

                float height = maxHeight * (float)JumpJetSize;
                if (height > maxHeight)
                    height = FloatHelper.Random(maxHeight - 0.5f, maxHeight);

                GL.PushMatrix();
                GL.Translate(TankSprite.Position.X, TankSprite.Position.Y, TankSprite.Position.Z - (height * 0.5f) + 0.25f);
               
                Sceene.TheModule.SetBillboard();
                JumpJet.DrawCenteredXZ(height, height);

                GL.PopMatrix();

                GL.Color4(1, 0.5, 0.5, 0.25);
                GL.PushMatrix();
                GL.Translate(TankSprite.Position.X, TankSprite.Position.Y, 0.0005);

                float factor = 1.0f / (TankSprite.Position.Z / ((float)ShotGlowRadius * 0.5f));
                ShotGlow.DrawCentered((float)ShotGlowRadius * 5 * factor, (float)ShotGlowRadius * 5 * factor);
                GL.PopMatrix();
                GL.Color4(1, 1, 1, 1.0);
            }


            GL.DepthMask(true);
            foreach (AnimationEffectInstance shot in Shots)
            {
                GL.PushMatrix();
                GL.Translate(shot.Position);
                Sceene.TheModule.SetBillboard();
                Shot.DrawCenteredXZ(2, 2);
                GL.PopMatrix();
            }

            GL.Enable(EnableCap.Lighting);
        }

    }

    public class Tank
    {
        public TankModel model;

        public Vector3 Position = Vector3.Zero;
        public double Rotation = 0;
        public double JumpVel = 0;
        public double Gravity = -15;

        // for when the tank is "flying"
        public Vector2 Velocity = Vector2.Zero;
        public double RotVel = 0;

        public Vector2 InputVectors = Vector2.Zero;
        public Vector2 Speeds = new Vector2(10, 120);

        public Tank ( TankModel m )
        {
            model = m;
        }

        public void Update ( double time, double delta )
        {
            Vector3 newPos = new Vector3();

            double newRot = Rotation;
            
            if (Position.Z <= 0)
            {
                RotVel = InputVectors.Y * Speeds.Y;                
                Vector2 heading = VectorHelper2.FromAngle(newRot);
                Velocity = new Vector2(heading.X * InputVectors.X * Speeds.X, heading.Y * InputVectors.X * Speeds.X);
            }

            newRot += RotVel * delta;
            newPos.X = Position.X + Velocity.X * (float)delta;
            newPos.Y = Position.Y + Velocity.Y * (float)delta;

            newPos.Z = Position.Z;
            if (JumpVel > 0)
            {
                JumpVel += Gravity * delta;
                newPos.Z += (float)JumpVel * (float)delta;
            }
            else
            {
                if (newPos.Z > 0) // falling
                {
                    JumpVel += Gravity * delta;
                    newPos.Z += (float)JumpVel * (float)delta;
                }
                
                if (newPos.Z <= 0)
                {
                    newPos.Z = 0;
                    JumpVel = 0;
                }
            }

            Position = newPos;
            Rotation = newRot;
        }

        public void Draw ( double time )
        {
            model.Draw(Position, (float)Rotation);
        }

        public void DrawTrans(double time)
        {
            model.DrawTrans(Position, (float)Rotation);
        }
    }

    public class TankModel
    {
        public StaticModel mesh;
        Texture skin, shadow;

        public TankModel(StaticModel m, Texture s, Texture sh)
        {
            mesh = m;
            skin = s;
            shadow = sh;
        }

        public void Draw(Vector3 position, float rotation)
        {
            GL.PushMatrix();
            GL.Translate(position.X, position.Y, position.Z + 0.01f);
            GL.Rotate(90, 1, 0, 0);
            GL.Rotate(rotation, 0, 1, 0);
            skin.Bind();
            mesh.drawAll(false);
            GL.PopMatrix();
        }

        public void DrawTrans(Vector3 position, float rotation)
        {
            GL.DepthMask(false);
            GL.Disable(EnableCap.Lighting);

            GL.PushMatrix();

            GL.Translate(position.X + 0.5, position.Y - 0.35, 0);
            GL.Rotate(rotation, 0, 0, 1);

            float shadowScale = 1.0f + (position.Z / 40.0f);
            if (shadowScale > 3.0f)
                shadowScale = 3.0f;

            Vector2 shadowSize = new Vector2(4.8f * shadowScale, 2.8f * shadowScale);

            shadow.Bind();

            GL.Disable(EnableCap.Lighting);

            GL.Begin(BeginMode.Quads);
            GL.Color4(1.0, 1.0, 1.0, 0.75);
            GL.Normal3(0, 0, 1.0);

            GL.TexCoord2(0, 1.0);
            GL.Vertex2(-shadowSize.X, -shadowSize.Y);

            GL.TexCoord2(1.0, 1.0);
            GL.Vertex2(shadowSize.X, -shadowSize.Y);

            GL.TexCoord2(1.0, 0);
            GL.Vertex2(shadowSize.X, shadowSize.Y);

            GL.TexCoord2(0, 0);
            GL.Vertex2(-shadowSize.X, shadowSize.Y);

            GL.End();
            GL.Enable(EnableCap.Lighting);

            GL.PopMatrix();
            GL.Color4(1.0, 1.0, 1.0, 1.0);

            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
        }
    }
}
