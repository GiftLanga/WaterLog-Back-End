using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterLog_Backend.Controllers;
using WaterLog_Backend.Models;

namespace WaterLog_Backend
{
    public class ControllerService : IControllerService
    {
        private MonitorsController monitorController;
        private ReadingsController readingsController;
        private SegmentEventsController segmentEventsController;
        private SegmentLeaksController segmentLeaksController;
        private SegmentsController segmentsController;
        private HistoryLogController historyLogController;

        public ControllerService(DatabaseContext context, IConfiguration config)
        {
           monitorController = new MonitorsController(context,config);
           readingsController = new ReadingsController(context, config,this);
           segmentEventsController = new SegmentEventsController(context,config);
           segmentLeaksController = new  SegmentLeaksController(context,config,this);
           segmentsController = new SegmentsController(context,config);
           historyLogController = new HistoryLogController(context,config);
        }

        public MonitorsController GetMonitorsController()
        {
            return monitorController;
        }

        public ReadingsController GetReadingsController()
        {
            return readingsController;
        }

        public SegmentEventsController GetSegmentEventsController()
        {
            return segmentEventsController;
        }

        public SegmentLeaksController GetSegmentLeaksController()
        {
            return segmentLeaksController;
        }

        public SegmentsController GetSegmentsController()
        {
            return segmentsController;
        }

        public HistoryLogController GetHistoryLogController()
        {
            return historyLogController;
        }
    }

    public interface IControllerService
    {
        MonitorsController GetMonitorsController();
        ReadingsController GetReadingsController();
        SegmentEventsController GetSegmentEventsController();
        SegmentLeaksController GetSegmentLeaksController();
        SegmentsController GetSegmentsController();
        HistoryLogController GetHistoryLogController();
    }
}
