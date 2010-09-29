using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using GraphTest;
using Drawables.Textures;
using Drawables.DisplayLists;
using Drawables.StaticModels;
using Drawables.StaticModels.OBJ;
using Math3D;

namespace P51Movie
{
    public class Module : GraphTest.MouseInspectionCameraModule
    {
        protected List<Sceene> Sceenes = new List<Sceene>();

        protected Sceene CurrentSceene = null;
        protected int Index = 0;

        public override string Name() { return "P51Movie"; }

        public override void Load(bool first)
        {
            base.Load(first);

            Sceenes.Add(new BZFlagDanceView(this.API,this));

            Index = 0;
            CurrentSceene = Sceenes[Index];
        }

        public override void SetGLOptons()
        {
            base.SetGLOptons();
            CurrentSceene.Load();
        }

        public override void EndFrame(double time)
        {
            if (CurrentSceene != null && Index != Sceenes.Count)
            {
                if (CurrentSceene.Done(time))
                {
                    Index++;
                    if (Index < Sceenes.Count)
                    {
                        CurrentSceene = Sceenes[Index];
                        CurrentSceene.Load();
                    }
                }
            }
        }

        public override void Draw3D(double time)
        {
            if (CurrentSceene != null)
                CurrentSceene.Draw3D(time);
        }

        public override void DrawOverlay(double time)
        {
            if (CurrentSceene != null)
                CurrentSceene.DrawOverlay(time);
        }
    }

    public class Sceene
    {
        public static ViewAPI API;
        public static Module TheModule;

        public Sceene( ViewAPI api, Module m)
        {
            API = api;
            TheModule = m;
        }

        public virtual bool Done(double time)
        {
            return false;
        }

        public virtual void Load()
        {
        }

        public virtual void Draw3D(double time)
        {

        }

        public virtual void DrawOverlay(double time)
        {

        }
    }

    public class BZFlagDanceView : Sceene
    {
        List<PathDrivenTank> Tanks = new List<PathDrivenTank>();

        Overlay TextOverlay = new Overlay();

        BZGround Ground;
        double LastUpdate = 0;
      
        public BZFlagDanceView(ViewAPI api, Module m) : base(api, m) { }

        protected void LoadTanks()
        {
            Texture shadow = TextureSystem.system.GetTexture(API.GetFile("model/shadow.png"));
            
            Texture blue = TextureSystem.system.GetTexture(API.GetFile("model/blue.png"));
            Texture red = TextureSystem.system.GetTexture(API.GetFile("model/red.png"));
            Texture green = TextureSystem.system.GetTexture(API.GetFile("model/green.png"));
            Texture purple = TextureSystem.system.GetTexture(API.GetFile("model/purple.png"));

            StaticModel mesh = OBJFile.Read(API.GetFile("model/tank.obj"));

            List<TankModel> models = new List<TankModel>();

            models.Add(new TankModel(mesh, red, shadow));
            models.Add(new TankModel(mesh, green, shadow));
            models.Add(new TankModel(mesh, blue, shadow));
            models.Add(new TankModel(mesh, purple, shadow));

            List<Texture> Shots = new List<Texture>();
            Shots.Add(TextureSystem.system.GetTexture(API.GetFile("img/shots/red.png")));
            Shots.Add(TextureSystem.system.GetTexture(API.GetFile("img/shots/green.png")));
            Shots.Add(TextureSystem.system.GetTexture(API.GetFile("img/shots/blue.png")));
            Shots.Add(TextureSystem.system.GetTexture(API.GetFile("img/shots/purple.png")));

            int count = 5;

            Random rand = new Random();

            float spread = 25.0f;

            for( int i = 0; i < count; i++)
            {
                int index = rand.Next(models.Count);
                if (index >= models.Count)
                    index = 0;

                Tank tank = new Tank(models[index]);
                PathDrivenTank SampleTank = new PathDrivenTank(tank, Shots[index]);

                SampleTank.InitalRot = FloatHelper.Random(360);
                SampleTank.InitalPos = new Vector3(FloatHelper.Random(-spread, spread), FloatHelper.Random(-spread, spread), 0);

                SampleTank.Segments.Add(new PathDrivenTank.PathSegment(PathDrivenTank.PathSegment.SegmentType.Delay, Vector2.Zero, FloatHelper.Random(3)));
                SampleTank.Segments.Add(new PathDrivenTank.PathSegment(PathDrivenTank.PathSegment.SegmentType.Drive, new Vector2(0, FloatHelper.Random(-1, 1)), FloatHelper.Random(1, 2)));
                SampleTank.Segments.Add(new PathDrivenTank.PathSegment(PathDrivenTank.PathSegment.SegmentType.Drive, new Vector2(1, FloatHelper.Random(-1, 1)), FloatHelper.Random(1, 3)));
                SampleTank.Segments.Add(new PathDrivenTank.PathSegment(PathDrivenTank.PathSegment.SegmentType.Jump, new Vector2(0, FloatHelper.Random(-1, 1)), 0));
                SampleTank.Segments.Add(new PathDrivenTank.PathSegment(PathDrivenTank.PathSegment.SegmentType.Delay, Vector2.Zero, FloatHelper.Random(-1,1)));
                SampleTank.Segments.Add(new PathDrivenTank.PathSegment(PathDrivenTank.PathSegment.SegmentType.Shoot, Vector2.Zero, 0));
                SampleTank.Segments.Add(new PathDrivenTank.PathSegment(PathDrivenTank.PathSegment.SegmentType.WaitTillLand, Vector2.Zero, 0));
                SampleTank.Segments.Add(new PathDrivenTank.PathSegment(PathDrivenTank.PathSegment.SegmentType.Shoot, Vector2.Zero, 0));
                SampleTank.Segments.Add(new PathDrivenTank.PathSegment(PathDrivenTank.PathSegment.SegmentType.Drive, new Vector2(-1, FloatHelper.Random(-1, 1)), 2));

                SampleTank.Loop = true;

                Tanks.Add(SampleTank);
            }           
        }

        protected void LoadText ()
        {
            TextOverlay = new Overlay();

            Overlay.OverlayDef seq = new Overlay.OverlayDef();
            seq.Kind = Overlay.OverlayDef.OverlayKind.Delay;
            seq.LifeTime = 3;
            TextOverlay.Sequence.Add(seq);

            seq = new Overlay.OverlayDef();
            seq.Kind = Overlay.OverlayDef.OverlayKind.Cut;
            seq.LifeTime = 10;
            seq.Position = new Vector2(0, 0);// -API.Height*0.4f);
            seq.TextColor = Color.White;
            seq.Centered = true;
            seq.CenterOrigin = true;
            seq.Font = new Font(FontFamily.GenericSansSerif, 32.0f);
            seq.Text = "Gameplay feel a little dated?";
            TextOverlay.Sequence.Add(seq);

            seq = new Overlay.OverlayDef();
            seq.Kind = Overlay.OverlayDef.OverlayKind.Cut;
            seq.LifeTime = 20;
            seq.Position = new Vector2(0, 0);// -API.Height*0.4f);
            seq.TextColor = Color.White;
            seq.Centered = true;
            seq.CenterOrigin = true;
            seq.Font = new Font(FontFamily.GenericSansSerif, 42.0f);
            seq.Text = "Tired of dancing around?";
            TextOverlay.Sequence.Add(seq);
        }

        public override void Load()
        {
            GL.ClearColor(Color.SkyBlue);
            Ground = new BZGround();
            Ground.Load();
            LoadTanks();
            LoadText();

            TheModule.ViewPosition = new Vector3(0, -1, 2);
            TheModule.Spin = 45;
            TheModule.Tilt = 15;
            TheModule.Pullback = 50;
        }

        protected void DrawBackground(double time)
        {
            TheModule.SetCamera(time, false);
            Background.Draw(time,API);
        }

        void Animate(double time)
        {
            double delta = 0;

            if (LastUpdate != 0)
                delta = time - LastUpdate;

            Ground.Animate(time);

            foreach(PathDrivenTank tank in Tanks)
                tank.Update(time, delta);

            TextOverlay.Update(time, delta);
            LastUpdate = time;
        }

        public override void Draw3D(double time)
        {
            Animate(time);
            Ground.Draw(time);

            foreach (PathDrivenTank tank in Tanks)
                tank.DrawTrans(time);
          
            foreach (PathDrivenTank tank in Tanks)
                tank.Draw(time);
        }

        public override void DrawOverlay(double time)
        {
            TextOverlay.Draw(time);
        }
    }
}
