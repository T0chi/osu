// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.Multi.Components
{
    public class ParticipantsList : MultiplayerComposite
    {
        public const float TILE_SIZE = 35;

        public override Axes RelativeSizeAxes
        {
            get => base.RelativeSizeAxes;
            set
            {
                base.RelativeSizeAxes = value;
                fill.RelativeSizeAxes = value;
            }
        }

        public new Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            set
            {
                base.AutoSizeAxes = value;
                fill.AutoSizeAxes = value;
            }
        }

        public FillDirection Direction
        {
            get => fill.Direction;
            set => fill.Direction = value;
        }

        private readonly FillFlowContainer fill;

        public ParticipantsList()
        {
            InternalChild = fill = new FillFlowContainer { Spacing = new Vector2(10) };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RoomID.BindValueChanged(_ => updateParticipants(), true);
        }

        [Resolved]
        private IAPIProvider api { get; set; }

        private GetRoomScoresRequest request;

        private void updateParticipants()
        {
            var roomId = RoomID.Value ?? 0;

            request?.Cancel();

            // nice little progressive fade
            int time = 500;

            foreach (var c in fill.Children)
            {
                c.Delay(500 - time).FadeOut(time, Easing.Out);
                time = Math.Max(20, time - 20);
                c.Expire();
            }

            if (roomId == 0) return;

            request = new GetRoomScoresRequest(roomId);
            request.Success += scores => Schedule(() =>
            {
                if (roomId != RoomID.Value)
                    return;

                fill.Clear();
                foreach (var s in scores)
                    fill.Add(new UserTile(s.User));

                fill.FadeInFromZero(1000, Easing.OutQuint);
            });

            api.Queue(request);
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            base.Dispose(isDisposing);
        }

        private class UserTile : CompositeDrawable, IHasTooltip
        {
            private readonly User user;

            public string TooltipText => user.Username;

            public UserTile(User user)
            {
                this.user = user;
                Size = new Vector2(TILE_SIZE);
                CornerRadius = 5f;
                Masking = true;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.FromHex(@"27252d"),
                    },
                    new UpdateableAvatar
                    {
                        RelativeSizeAxes = Axes.Both,
                        User = user,
                    },
                };
            }
        }
    }
}
