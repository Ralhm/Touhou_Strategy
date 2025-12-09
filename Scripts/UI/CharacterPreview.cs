using Godot;
using System;

public partial class CharacterPreview : Control
{

    [Export]
    public AnimatedSprite2D Sprite;

    public bool EasingIn;
    public bool EasingOut;

    [Export]
    public bool Flip = false;


    [Export]
    public float Speed = 20.0f;

    [Export]
    public PreviewBar Bar;


    public Vector2 StartingPos;

    public Vector2 OffScreenPos;

    public override void _Ready()
    {
        base._Ready();
        Sprite = GetNode<AnimatedSprite2D>("Sprite");

        Sprite.FlipH = Flip;

        StartingPos = GlobalPosition;

        if (Flip)
        {
            OffScreenPos = new Vector2(StartingPos.X + 150, StartingPos.Y);
        }
        else
        {
            OffScreenPos = new Vector2(StartingPos.X - 150, StartingPos.Y);
        }

        BeginEaseOut();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (EasingIn)
        {
            EaseIn((float)delta);
        }
        else if (EasingOut)
        {
            EaseOut((float)delta);

        }



    }

    public void EaseIn(float dt)
    {
        float step = Speed * dt;

        Position = Position.Lerp(StartingPos, step);

        if (Position == StartingPos)
        {
            EasingIn = false;
        }
    }

    public void EaseOut(float dt)
    {
        float step = Speed * dt;

        Position = Position.Lerp(OffScreenPos, step);

        if (Position == OffScreenPos)
        {
            EasingOut = false;
        }
    }

    public void BeginEaseIn()
    {
        EasingOut = false;
        EasingIn = true;
    }

    public void BeginEaseOut()
    {
        EasingIn = false;
        EasingOut = true;
    }


    public void Enter(SpriteFrames sprites, int FinalHealth, int StartingHealth, int MaxHealth) 
    {
        Sprite.SpriteFrames = sprites;
        Bar.SetMaxBars(MaxHealth);
        Bar.SetBars(FinalHealth, StartingHealth);
        BeginEaseIn();
    }

    public void Enter(Character attacker, Character target)
    {
        Sprite.SpriteFrames = attacker.Sprite.SpriteFrames;
        Bar.SetMaxBars(attacker.Data.MaxHealth);


        if (target.CanCounter(attacker))
        {
            Bar.SetBars(attacker.GetProjectedHealth(target), attacker.Health);
        }
        else
        {
            Bar.SetBars(attacker.Health, attacker.Health);
        }

            BeginEaseIn();
    }


    public void SetSprite(SpriteFrames sprites)
    {
        Sprite.SpriteFrames = sprites;
        BeginEaseIn();
    }

}
