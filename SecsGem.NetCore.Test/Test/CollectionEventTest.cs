using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Event.Client;
using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Test.Test
{
    public class CollectionEventTest : SecsGemTestBase
    {
        public CollectionEvent CE { get; set; }

        public override async Task Setup()
        {
            await base.Setup();

            CE = new CollectionEvent
            {
                Id = 1,
                Enabled = true,
                Name = "Test",
                CollectionReports = new List<CollectionReport>
                {
                    new() {
                        Id = 2,
                        DataVariables = new List<DataVariable>
                        {
                            new() {
                                Id = "Test1",
                                Description = "Test1",
                                Unit = "/g",
                                Value = "101"
                            }
                        }
                    },
                     new() {
                        Id = 3,
                        DataVariables = new List<DataVariable>
                        {
                            new() {
                                Id = "Test2",
                                Description = "Test2",
                                Unit = "/t",
                                Value = "102"
                            }
                        }
                    }
                }
            };
            _server.Feature.CollectionEvents.Add(CE);
            _server.Feature.CollectionReports.AddRange(CE.CollectionReports);
            _server.Feature.DataVariables.AddRange(CE.CollectionReports.SelectMany(x => x.DataVariables));
        }

        [Test]
        public async Task Get_Collection_Event_Detail()
        {
            var ces = await _client.Function.CollectionEventDefinitionGet();
            Assert.Multiple(() =>
            {
                Assert.That(ces.Count(), Is.EqualTo(1));

                var fce = ces.First();
                Assert.That(fce.CollectionReports, Has.Count.EqualTo(1));
                Assert.That(fce.Id, Is.EqualTo(CE.Id));
                Assert.That(fce.Name, Is.EqualTo(CE.Name));

                var fcr = fce.CollectionReports.First();
                Assert.That(fcr.DataVariables, Has.Count.EqualTo(2));

                var sdvs = CE.CollectionReports.SelectMany(x => x.DataVariables).ToList();
                for (var i = 0; i < 2; i++)
                {
                    var dv = fcr.DataVariables[i];
                    var sdv = sdvs[i];

                    Assert.That(dv.Id, Is.EqualTo(sdv.Id));
                }
            });
        }

        [Test]
        public async Task Get_Collection_Event_Detail_Invalid_Id()
        {
            var ces = await _client.Function.CollectionEventDefinitionGet(new uint[] { 2 });
            Assert.Multiple(() =>
            {
                Assert.That(ces.Count(), Is.EqualTo(1));

                var fce = ces.First();
                Assert.That(fce.CollectionReports.SelectMany(x => x.DataVariables).Count(), Is.EqualTo(0));
                Assert.That(fce.Id, Is.EqualTo(2));
                Assert.That(fce.Name, Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public async Task Get_Collection_Event_Link()
        {
            var ceLinks = await _client.Function.CollectionEventLinkGet();
            Assert.That(ceLinks.Count(), Is.EqualTo(1));

            var fce = ceLinks.First();
            Assert.That(fce.CollectionReports.Count, Is.EqualTo(2));

            Assert.Multiple(() =>
            {
                Assert.That(fce.Id, Is.EqualTo(CE.Id));
                Assert.That(fce.Name, Is.EqualTo(CE.Name));

                for (var i = 0; i < 2; i++)
                {
                    Assert.That(fce.CollectionReports[i].Id, Is.EqualTo(CE.CollectionReports[i].Id));
                }
            });
        }

        [Test]
        public async Task Get_Collection_Event_Link_Invalid_Id()
        {
            var ceLinks = await _client.Function.CollectionEventLinkGet(new uint[] { 2 });

            Assert.Multiple(() =>
            {
                Assert.That(ceLinks.Count(), Is.EqualTo(1));

                var fce = ceLinks.First();
                Assert.That(fce.CollectionReports, Is.Empty);
            });
        }

        [Test]
        public async Task Get_Collection_Report_Detail()
        {
            var crs = await _client.Function.CollectionReportDefinitionGet();
            Assert.That(crs.Count(), Is.EqualTo(2));

            var scrs = CE.CollectionReports.ToList();
            for (var i = 0; i < 2; i++)
            {
                var cr = crs.ElementAt(i);
                var scr = scrs[i];

                Assert.That(cr.Id, Is.EqualTo(scr.Id));
                Assert.That(cr.DataVariables, Has.Count.EqualTo(scr.DataVariables.Count));

                for (var j = 0; j < cr.DataVariables.Count; j++)
                {
                    Assert.That(cr.DataVariables[j].Id, Is.EqualTo(scr.DataVariables[j].Id));
                }
            }
        }

        [Test]
        public async Task Get_Collection_Report_Detail_Invalid_Id()
        {
            var crs = await _client.Function.CollectionReportDefinitionGet(new uint[] { 5 });

            Assert.Multiple(() =>
            {
                Assert.That(crs.Count(), Is.EqualTo(1));

                var fcr = crs.First();
                Assert.That(fcr.DataVariables, Is.Empty);
            });
        }

        [Test]
        public async Task Get_Enabled_Collection_Event()
        {
            var ces = await _client.Function.CollectionEventEnabledIdGet();
            Assert.That(ces.Count, Is.EqualTo(1));
            Assert.That(ces.First(), Is.EqualTo(1));

            _server.Feature.CollectionEvents.First().Enabled = false;
            ces = await _client.Function.CollectionEventEnabledIdGet();
            Assert.That(ces.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task Collection_Event_Report_Define()
        {
            var ack = await _client.Function.CollectionReportDefine(1, new List<CollectionReport>() {
                new() {
                    Id = 2,
                    DataVariables = new()
                    {
                        new DataVariable
                        {
                            Id = "Test1"
                        }
                    }
                }
            });
            Assert.That(ack, Is.EqualTo(SECS_RESPONSE.DRACK.AlreadyDefined));

            ack = await _client.Function.CollectionReportDefine(1, new List<CollectionReport>() {
                new() {
                    Id = 1,
                    DataVariables = new()
                    {
                        new DataVariable
                        {
                            Id = "Test3"
                        }
                    }
                }
            });
            Assert.That(ack, Is.EqualTo(SECS_RESPONSE.DRACK.InvalidVid));

            ack = await _client.Function.CollectionReportDefine(1, new List<CollectionReport>() {
                new() {
                    Id = 1,
                    DataVariables = new()
                    {
                    }
                }
            });
            Assert.That(ack, Is.EqualTo(SECS_RESPONSE.DRACK.InvalidFormat));

            ack = await _client.Function.CollectionReportDefine(1, new List<CollectionReport>() {
                new() {
                    Id = 1,
                    DataVariables = new()
                    {
                         new DataVariable
                        {
                            Id = "Test1"
                        }
                    }
                }
            });
            Assert.That(ack, Is.EqualTo(SECS_RESPONSE.DRACK.Ok));
            Assert.That(_server.Feature.CollectionReports.Count, Is.EqualTo(3));
            var nrp = _server.Feature.CollectionReports.FirstOrDefault(x => x.Id == 1);
            Assert.That(nrp, Is.Not.Null);
            Assert.That(nrp.DataVariables, Has.Count.EqualTo(1));
            var ndv = nrp.DataVariables.First();
            Assert.That(ndv.Id, Is.EqualTo("Test1"));
        }

        [Test]
        public async Task Collection_Event_Report_Link()
        {
            await _client.Function.CollectionReportDefine(1, new List<CollectionReport>() {
                new() {
                    Id = 1,
                    DataVariables = new()
                    {
                         new DataVariable
                        {
                            Id = "Test1"
                        }
                    }
                }
            });

            var ack = await _client.Function.CollectionReportLink(1, new List<CollectionEvent> {
                new() {
                    Id = 2,
                    CollectionReports = new List<CollectionReport>
                    {
                        new() {
                            Id = 1,
                        }
                    }
                }
            });
            Assert.That(ack, Is.EqualTo(SECS_RESPONSE.LRACK.OneOrMoreCeidInvalid));

            ack = await _client.Function.CollectionReportLink(1, new List<CollectionEvent> {
                new() {
                    Id = 1,
                    CollectionReports = new List<CollectionReport>
                    {
                        new() {
                            Id = 10,
                        }
                    }
                }
            });
            Assert.That(ack, Is.EqualTo(SECS_RESPONSE.LRACK.OneOrMoreRptidInvalid));

            ack = await _client.Function.CollectionReportLink(1, new List<CollectionEvent> {
                new() {
                    Id = 1,
                    CollectionReports = new List<CollectionReport>
                    {
                        new() {
                            Id = 2,
                        }
                    }
                }
            });
            Assert.That(ack, Is.EqualTo(SECS_RESPONSE.LRACK.OneOrMoreCeidAlreadyDefined));

            ack = await _client.Function.CollectionReportLink(1, new List<CollectionEvent> {
                new() {
                    Id = 1,
                    CollectionReports = new List<CollectionReport>
                    {
                        new() {
                            Id = 1,
                        }
                    }
                }
            });
            Assert.That(ack, Is.EqualTo(SECS_RESPONSE.LRACK.Ok));

            var sce = _server.Feature.CollectionEvents.First();
            Assert.That(sce.CollectionReports, Has.Count.EqualTo(3));
            var srp = sce.CollectionReports.FirstOrDefault(x => x.Id == 1);
            Assert.That(srp, Is.Not.Null);
            Assert.That(srp.DataVariables, Has.Count.EqualTo(1));
            var sdv = srp.DataVariables.First();
            Assert.That(sdv.Id, Is.EqualTo("Test1"));
        }

        [Test]
        public async Task Collection_Event_Send()
        {
            CollectionEvent ce = null;
            _client.OnEvent += async (sender, evt) =>
            {
                if (evt is SecsGemCollectionEventEvent nevt)
                {
                    ce = nevt.CollectionEvent;
                }
            };

            _server.OnEvent += async (sender, evt) =>
            {
                if (evt is SecsGemGetDataVariableEvent nevt)
                {
                    foreach (var item in nevt.Params)
                    {
                        item.Value = item.Description + " Value";
                    }
                }
            };

            await _server.Function.SendCollectionEvent(1);

            Assert.That(ce, Is.Not.Null);
            Assert.That(ce.CollectionReports, Has.Count.EqualTo(2));

            var sdvs = CE.CollectionReports.SelectMany(x => x.DataVariables).ToList();

            ce.CollectionReports.ForEach(rp =>
            {
                Assert.That(rp.DataVariables, Has.Count.EqualTo(1));
                var dv = rp.DataVariables.First();
                var sdv = sdvs[0];
                sdvs.RemoveAt(0);
                Assert.That(dv.Value, Is.EqualTo(sdv.Description + " Value"));
            });
        }
    }
}