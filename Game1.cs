using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace prog_spel
    {
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Texture2D coin;
        Texture2D Boss;
        Texture2D model1;
        Texture2D Gubbe;
        Vector2 Gubbe_pos;
        Vector2 Gubbe_vel;
        Vector2 model1_pos;
        Vector2 model1_vel;
        Vector2 Boss_pos;
        Vector2 Boss_vel;
        List<Vector2> coin_pos_list = new List<Vector2>();
        Rectangle rec_Gubbe;
        Rectangle rec_coin;
        int coinsCollected = 0;
        int coinCount = 0;
        int health = 500;
        bool hit = false;
        Random slump = new Random();
        private SpriteFont spriteFont;
        private int totalCoinsCollected = 0;
        private float model1Timer = 0f;
        private const float model1MovementDuration = 2f;
        bool isBossActive = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            _graphics.PreferredBackBufferWidth = 1024; // Set the preferred width
            _graphics.PreferredBackBufferHeight = 768; // Set the preferred height
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

        }

        protected override void Initialize()
        {


            Gubbe_vel.X = 10f;
            Gubbe_vel.Y = 8f;

            model1_pos.X = 500;
            model1_pos.Y = 100;

            Gubbe_pos = GetRandomPositionWithinBounds(50); // Get random position for Gubbe
            model1_pos = GetRandomPositionAwayFromGubbe(Gubbe_pos, 300); // Get random position for model1 away from Gubbe
            SpawnCoins();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            coin = Content.Load<Texture2D>("Bilder/coin (1)");
            Boss = Content.Load<Texture2D>("Bilder/Boss");
            model1 = Content.Load<Texture2D>("Bilder/model1");
            Gubbe = Content.Load<Texture2D>("Bilder/Gubbe");
            spriteFont = Content.Load<SpriteFont>("Fonts/File");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (totalCoinsCollected >= 30 && !isBossActive)
            {
                isBossActive = true; // Activate the boss
                model1_pos = new Vector2(-100, -100); // Move the model1 off-screen (you can adjust the position as needed)
            }


            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.W))
                Gubbe_pos.Y -= Gubbe_vel.Y; // Subtract Gubbe_vel.Y to move up
            if (keyboardState.IsKeyDown(Keys.S))
                Gubbe_pos.Y += Gubbe_vel.Y; // Add Gubbe_vel.Y to move down
            if (keyboardState.IsKeyDown(Keys.A))
                Gubbe_pos.X -= Gubbe_vel.X; // Subtract Gubbe_vel.X to move left
            if (keyboardState.IsKeyDown(Keys.D))
                Gubbe_pos.X += Gubbe_vel.X; // Add Gubbe_vel.X to move right

            if (Gubbe_pos.X > Window.ClientBounds.Width - (Gubbe.Width * 0.2f))
                Gubbe_pos.X = Window.ClientBounds.Width - (Gubbe.Width * 0.2f);

            if (Gubbe_pos.X < 0)
                Gubbe_pos.X = 0;

            if (Gubbe_pos.Y > Window.ClientBounds.Height - (Gubbe.Height * 0.2f))
                Gubbe_pos.Y = Window.ClientBounds.Height - (Gubbe.Height * 0.2f);

            if (Gubbe_pos.Y < 0)
                Gubbe_pos.Y = 0;

            rec_Gubbe = new Rectangle(Convert.ToInt32(Gubbe_pos.X), Convert.ToInt32(Gubbe_pos.Y), (int)(Gubbe.Width * 0.2f), (int)(Gubbe.Height * 0.2f));

            foreach (Vector2 cn in coin_pos_list.ToList())
            {
                rec_coin = new Rectangle(Convert.ToInt32(cn.X), Convert.ToInt32(cn.Y), (int)(coin.Width * 0.8f), (int)(coin.Height * 0.8f));
                hit = CheckCollision(rec_Gubbe, rec_coin);

                if (hit == true)
                {
                    coin_pos_list.Remove(cn);
                    coinsCollected++;
                    totalCoinsCollected++;
                    hit = false;
                }
            }

            coinCount = coinsCollected;

            if (coinCount % 5 == 0 && coinsCollected > 0)
            {
                SpawnCoins();
                coinsCollected = 0;
            }
            model1_pos += model1_vel;


            Rectangle rec_model1 = new Rectangle(Convert.ToInt32(model1_pos.X), Convert.ToInt32(model1_pos.Y), (int)(model1.Width * 0.2f), (int)(model1.Height * 0.2f));
            if (rec_Gubbe.Intersects(rec_model1))
            {

                health--;

                model1_pos.X = Window.ClientBounds.Width - model1.Width;
                model1_pos.Y = slump.Next(0, Window.ClientBounds.Height - model1.Height);
            }
            if (health <= 0)
            {
                // End the game
                Exit();
            }
            // Calculate the distance between model1 and Gubbe
            float distanceToGubbe = Vector2.Distance(model1_pos, Gubbe_pos);

            // Check if model1 should follow Gubbe
            if (distanceToGubbe <= 200)
            {
                // Calculate the direction vector towards Gubbe
                Vector2 directionToGubbe = Gubbe_pos - model1_pos;
                directionToGubbe.Normalize();

                // Update the velocity of model1 to follow Gubbe
                model1_vel = directionToGubbe * 2f;

                // Reset the timer when model1 can see Gubbe
                model1Timer = 0f;
            }
            else
            {
                // Increment the timer when model1 cannot see Gubbe
                model1Timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Check if the timer has exceeded the movement duration
                if (model1Timer >= model1MovementDuration)
                {
                    // Generate random direction for model1
                    float directionX = (float)slump.NextDouble() - 0.5f; // Random value between -0.5 and 0.5
                    float directionY = (float)slump.NextDouble() - 0.5f; // Random value between -0.5 and 0.5
                    Vector2 direction = new Vector2(directionX, directionY);
                    direction.Normalize();

                    // Update the velocity of model1 to move randomly
                    model1_vel = direction * 2f;

                    // Reset the timer
                    model1Timer = 0f;
                }
            }

            if (model1_pos.X < 0)
                model1_pos.X = 0;
            if (model1_pos.X > Window.ClientBounds.Width - (model1.Width * 0.2f))
                model1_pos.X = Window.ClientBounds.Width - (model1.Width * 0.2f);
            if (model1_pos.Y < 0)
                model1_pos.Y = 0;
            if (model1_pos.Y > Window.ClientBounds.Height - (model1.Height * 0.2f))
                model1_pos.Y = Window.ClientBounds.Height - (model1.Height * 0.2f);
            // Update the position of model1 based on velocity
            model1_pos += model1_vel;

            if (model1_pos.X < 0 || model1_pos.X > Window.ClientBounds.Width - (model1.Width * 0.2f))
            {
                // Reverse the X velocity to change direction instantly
                model1_vel.X *= -1;
            }
            if (model1_pos.Y < 0 || model1_pos.Y > Window.ClientBounds.Height - (model1.Height * 0.2f))
            {
                // Reverse the Y velocity to change direction instantly
                model1_vel.Y *= -1;
            }

            if (isBossActive)
            {
                // Calculate the direction vector towards the player
                Vector2 directionToPlayer = Gubbe_pos - Boss_pos;
                directionToPlayer.Normalize();

                // Update the boss position to follow the player
                Boss_pos += directionToPlayer * Boss_vel;

                // Additional boss behavior can be added here
            }
            // Check collision between Gubbe and coins
            for (int i = 0; i < coin_pos_list.Count; i++)
            {
                rec_coin = new Rectangle((int)coin_pos_list[i].X, (int)coin_pos_list[i].Y, coin.Width, coin.Height);
                rec_Gubbe = new Rectangle((int)Gubbe_pos.X, (int)Gubbe_pos.Y, Gubbe.Width, Gubbe.Height);

                if (rec_Gubbe.Intersects(rec_coin))
                {
                    coinsCollected++;
                    coin_pos_list.RemoveAt(i);
                    i--;
                }
            }

            // Check collision between Gubbe and model1
            rec_Gubbe = new Rectangle((int)Gubbe_pos.X, (int)Gubbe_pos.Y, Gubbe.Width, Gubbe.Height);
            Rectangle rec_model1 = new Rectangle((int)model1_pos.X, (int)model1_pos.Y, model1.Width, model1.Height);

            if (rec_Gubbe.Intersects(rec_model1))
            {
                health -= 50;
                hit = true;
                Gubbe_pos = GetRandomPositionWithinBounds(50); // Reset Gubbe position
            }

            // Check collision between Gubbe and Boss (if Boss is active)
            if (isBossActive)
            {
                Rectangle rec_Boss = new Rectangle((int)Boss_pos.X, (int)Boss_pos.Y, Boss.Width, Boss.Height);

                if (rec_Gubbe.Intersects(rec_Boss))
                {
                    health -= 100;
                    hit = true;
                    Gubbe_pos = GetRandomPositionWithinBounds(50); // Reset Gubbe position
                }
            }

            // Update the model1 position and timer
            model1Timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (model1Timer >= model1MovementDuration)
            {
                model1_pos = GetRandomPositionAwayFromGubbe(Gubbe_pos, 300);
                model1Timer = 0f;
            }

            // Update the Boss position and timer (if Boss is active)
            if (isBossActive)
            {
                Boss_pos.X += Boss_vel.X;
                Boss_pos.Y += Boss_vel.Y;

                if (Boss_pos.X <= 0 || Boss_pos.X >= _graphics.PreferredBackBufferWidth - Boss.Width)
                    Boss_vel.X *= -1;
                if (Boss_pos.Y <= 0 || Boss_pos.Y >= _graphics.PreferredBackBufferHeight - Boss.Height)
                    Boss_vel.Y *= -1;
            }

            // Check if Gubbe's health reaches 0
            if (health <= 0)
            {
                // Game over logic here
                // Reset the game or display game over screen
            }



            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            _spriteBatch.Draw(Gubbe, Gubbe_pos, null, Color.White, 0f, Vector2.Zero, 0.20f, SpriteEffects.None, 0f);

            foreach (Vector2 cn in coin_pos_list)
            {
                Rectangle coinRect = new Rectangle(Convert.ToInt32(cn.X), Convert.ToInt32(cn.Y), (int)(coin.Width * 0.8f), (int)(coin.Height * 0.8f));
                _spriteBatch.Draw(coin, coinRect, Color.White);
            }
            if (!isBossActive)
            {
                _spriteBatch.Draw(model1, model1_pos, null, Color.White, 0f, Vector2.Zero, 0.2f, SpriteEffects.None, 0f);
            }
            if (isBossActive)
            {
                _spriteBatch.Draw(Boss, Boss_pos, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }



            string text = "Lives: " + health + "   Coins: " + totalCoinsCollected;
            Vector2 textPosition = new Vector2(10, 10);
            _spriteBatch.DrawString(spriteFont, text, textPosition, Color.White);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
 
            public bool CheckCollision(Rectangle gubbe, Rectangle coin)
            {
                return gubbe.Intersects(coin);
            }

            private void SpawnCoins()
            {
                coin_pos_list.Clear(); // Clear the list of existing coins

                for (int i = 0; i < 5; i++)
                {
                    // Generate a new coin position
                    Vector2 newCoinPos;
                    bool isOverlapping;

                    do
                    {
                        isOverlapping = false;

                        // Generate random position
                        newCoinPos.X = slump.Next(0, Window.ClientBounds.Width - 50);
                        newCoinPos.Y = slump.Next(0, Window.ClientBounds.Height - 50);

                        // Check if the new coin position is too close to any existing coins
                        foreach (Vector2 existingPos in coin_pos_list)
                        {
                            if (Vector2.Distance(newCoinPos, existingPos) < 160)
                            {
                                isOverlapping = true;
                                break;
                            }
                        }

                        // Check if the new coin position is too close to the Gubbe character
                        if (Vector2.Distance(newCoinPos, Gubbe_pos) < 50)
                        {
                            isOverlapping = true;
                        }
                        if (Vector2.Distance(newCoinPos, model1_pos) < 50)
                        {
                            isOverlapping = true;
                        }
                    }
                    while (isOverlapping);

                    // Add the non-overlapping coin position to the list
                    coin_pos_list.Add(newCoinPos);
                }
            }
            private Vector2 GetRandomPositionWithinBounds(int padding)
            {
                int minX = padding;
                int minY = padding;
                int maxX = Window.ClientBounds.Width - padding;
                int maxY = Window.ClientBounds.Height - padding;

                Vector2 position = new Vector2(slump.Next(minX, maxX), slump.Next(minY, maxY));
                return position;
            }

            private Vector2 GetRandomPositionAwayFromGubbe(Vector2 gubbePosition, float minDistance)
            {
                Vector2 position;
                float distance;

                do
                {
                    position = GetRandomPositionWithinBounds(50); // Get a randoms position
                    distance = Vector2.Distance(position, gubbePosition); // Calculate the distance between the position and Gubbe
                }
                while (distance < minDistance); // Repeat until the position is far enough from Gubbe

                return position;
            }

        }
    }
